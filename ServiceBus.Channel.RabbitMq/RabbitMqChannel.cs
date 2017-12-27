namespace ServiceBus.Channel.RabbitMq {
    using Infra.Entities;
    using Infra.Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;

    public class RabbitMqChannel : IChannel {
        private IConnection _connection;
        private IModel _channel;
        private readonly Config _config;
        public event EventHandler<MessageReceivedEventArgs> OnMessageReceived;
        public string ChannelId { get; }

        public RabbitMqChannel() : this(@"Config\RabbitChannelConfig.json".FromJsonFile<Config>()) { }

        public RabbitMqChannel(Config config) {
            _config = config;
            ChannelId = Guid.NewGuid().ToString();
        }

        public void Publish(string subject, MessageData data) {
            if (_channel == null) {
                throw new Exception("You must run SetUp() first");
            }
            var props = _channel.CreateBasicProperties();
            props.ContentType = data.ContentType;
            props.MessageId = data.MessageId;
            if (data.CorrelationId != null) {
                props.CorrelationId = data.CorrelationId;
            }
            props.Headers = new Dictionary<string, object>();
            foreach (var kv in data.Headers) {
                if (kv.Value != null) {
                    props.Headers.Add(kv.Key, kv.Value);
                }
            }
            _channel.BasicPublish(_config.ExchangeName,
                subject,
                props,
                data.Body);
        }

        public MessageData Request(string subject, MessageData data, TimeSpan timeOut) {
            throw new NotImplementedException();
        }

        public void SetUp() {
            CreateConnection();
        }

        public void CreateConnection() {
            var factory = new ConnectionFactory {
                UserName = _config.UserLogin,
                Password = _config.Password,
                HostName = _config.ServerName,
                VirtualHost = _config.VirtualHost ?? "/",
                Port = _config.Port ?? AmqpTcpEndpoint.UseDefaultPort
            };
            var version = FileVersionInfo.GetVersionInfo(factory.GetType().Assembly.Location).FileVersion;
            var connName = $"{_config.QueueName ?? ""}@{Environment.MachineName} (version: {version})";
            _connection = factory.CreateConnection(connName);
            _channel = _connection.CreateModel();
            _channel.BasicQos(0, 1, true);
            if (_config.DeclareExchange) {
                _channel.ExchangeDeclare(_config.ExchangeName,
                    _config.ExchangeType ?? ExchangeType.Direct,
                    _config.ExchangeDurable,
                    _config.ExchangeAutoDelete);
            }
            if (_config.DeclareQueue) {
                _channel.QueueDeclare(_config.QueueName,
                    _config.QueueDurable,
                    false,
                    _config.QueueAutoDelete,
                    null);
            }
        }

        public void AddBinders(IDictionary<string, MethodMetadata> binders) {
            if (_config.QueueName == null) return;

            var consumer = new EventingBasicConsumer(_channel) {ConsumerTag = _config.QueueName};

            foreach (var binder in binders) {
                var routingKey = binder.Key;
                _channel.QueueBind(_config.QueueName, _config.ExchangeName, routingKey, null);
            }
            consumer.Received += (model, ea) => {
                try {
                    var msg = CreateMessageData(ea);
                    foreach (var binder in binders) {
                        if (binder.Value?.MethodInfo != null &&
                            !ea.RoutingKey.EndsWith(binder.Value.MethodInfo.Name)) {
                            continue;
                        }
                        OnMessageReceived?.Invoke(this, MessageReceivedEventArgs.Create(binder.Value, msg));
                    }
                }
                catch (Exception ex) {
                    throw new Exception("Message received error", ex);
                }
                finally {
                    _channel.BasicAck(ea.DeliveryTag, true);
                }
            };
            _channel.BasicConsume(_config.QueueName, false, consumer.ConsumerTag, consumer);
        }

        private static MessageData CreateMessageData(BasicDeliverEventArgs ea) {
            var msg = new MessageData {
                Body = ea.Body,
                ContentEncoding = ea.BasicProperties.ContentEncoding,
                ContentType = ea.BasicProperties.ContentType,
                CorrelationId = ea.BasicProperties.CorrelationId,
                DeliveryMode = ea.BasicProperties.DeliveryMode,
                Expiration = ea.BasicProperties.Expiration,
                MessageId = ea.BasicProperties.MessageId,
                Persistent = ea.BasicProperties.Persistent,
                Priority = ea.BasicProperties.Priority,
                ReplyTo = ea.BasicProperties.ReplyTo,
                Type = ea.BasicProperties.Type,
                UserId = ea.BasicProperties.UserId,
                Headers = new Dictionary<string, string>()
            };
            if (ea.BasicProperties.Headers == null)
                return msg;

            foreach (var kv in ea.BasicProperties.Headers) {
                if (kv.Value != null) {
                    msg.Headers.Add(kv.Key, Encoding.UTF8.GetString((byte[]) kv.Value));
                }
            }
            return msg;
        }

        public void Close() {
            _channel?.Dispose();
            _connection?.Close();
        }
    }
}