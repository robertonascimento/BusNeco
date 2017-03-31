namespace ServiceBus.Channel.RabbitMq
{
    using Infra.Entities;
    using Infra.Interfaces;
    using System;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;

    public class RabbitMqChannel : IChannel
    {
        private readonly IModuleCatalog _moduleCatalog;
        private IConnection _connection;
        private IModel _channel;
        private readonly Config _config;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        public RabbitMqChannel(IModuleCatalog catalog)
        {
            _moduleCatalog = catalog;
            _config = @"Config\RabbitChannelConfig.json".FromJsonFile<Config>();
        }

        public void Publish(string topic, IMessageData data)
        {
            var props = _channel.CreateBasicProperties();
            props.CorrelationId = Guid.NewGuid().ToString();
            props.ContentType = data.ContentType;
            _channel.BasicPublish(exchange: _config.ExchangeName,
                                 routingKey: $"{_config.ExchangeName}.{topic}",
                                 basicProperties: props,
                                 body: data.Body);
        }

        public void SetUp()
        {
            CreateConnection();
            AddResponders();
            AddListeners();
        }

        public void Close()
        {
            _channel?.Dispose();
            _connection?.Close();
        }

        private void CreateConnection()
        {
            var factory = new ConnectionFactory
            {
                UserName = _config.UserLogin,
                Password = _config.Password,
                HostName = _config.ServerName,
                Port = AmqpTcpEndpoint.UseDefaultPort
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(_config.ExchangeName, ExchangeType.Topic);
            _channel.QueueDeclare(_moduleCatalog.ModuleName, false, false, false, null);
        }

        private void AddResponders()
        {
            foreach (var listener in _moduleCatalog.Responders)
            {
                var routingKey = listener.Key.StartsWith(_config.ExchangeName) ? listener.Key : $"{_config.ExchangeName}.{listener.Key}";
                routingKey = routingKey.Replace("@", _moduleCatalog.ModuleName);
                _channel.QueueBind(_moduleCatalog.ModuleName, _config.ExchangeName, routingKey, null);
            }
        }

        private void AddListeners()
        {
            foreach (var listener in _moduleCatalog.Listeners)
            {
                var routingKey = listener.Key.StartsWith(_config.ExchangeName) ? listener.Key : $"{_config.ExchangeName}.{listener.Key}";
                _channel.QueueBind(_moduleCatalog.ModuleName, _config.ExchangeName, routingKey, null);
                var consumer = new EventingBasicConsumer(_channel);
                _channel.BasicConsume(queue: _moduleCatalog.ModuleName, noAck: false, consumer: consumer);
                consumer.Received += (model, ea) =>
                {
                    var msg = new MessageData
                    {
                        Body = ea.Body,
                        ContentEncoding = ea.BasicProperties.ContentEncoding,
                        ContentType = ea.BasicProperties.ContentType,
                        CorrelationId = ea.BasicProperties.CorrelationId,
                        DeliveryMode = ea.BasicProperties.DeliveryMode,
                        Expiration = ea.BasicProperties.Expiration,
                        Headers = ea.BasicProperties.Headers,
                        MessageId = ea.BasicProperties.MessageId,
                        Persistent = ea.BasicProperties.Persistent,
                        Priority = ea.BasicProperties.Priority,
                        ReplyTo = ea.BasicProperties.ReplyTo,
                        Type = ea.BasicProperties.Type,
                        UserId = ea.BasicProperties.UserId
                    };
                    try
                    {
                        MessageReceived?.Invoke(this, MessageReceivedEventArgs.Create(listener.Value, msg));
                        _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Message received error", ex);
                    }

                };
            }
        }

    }
}