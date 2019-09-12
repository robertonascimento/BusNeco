
using System.Threading;

namespace ServiceBus
{
    using System.Collections.Generic;
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using Infra.Entities;
    using Infra.Enums;
    using Infra.Interfaces;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    
    public class Conector : IConector
    {
        private readonly IChannel _channel;
        private readonly MessageEncodingType _encondingTypeDefault;
        private const string CALLBACK = "callback";
        private const string NO_REPLY = "@no-reply";
        private const string EXPECTED_ARGUMENT_TYPE = "ExpectedArgumentType";
        private const string ACCEPT_ENCONDING = "accept-enconding";
        private readonly IDictionary<string, MessageData> _callbackObj;
        private HandlerCatalog _handlerCatalog;

        public Conector(IChannel channel, MessageEncodingType encondingTypeDefault)
        {
            _channel = channel;
            _encondingTypeDefault = encondingTypeDefault;
            _callbackObj = new ConcurrentDictionary<string, MessageData>();
        }

        public void SetUp(HandlerCatalog catalog)
        {
            _channel.SetUp();
            _handlerCatalog = catalog;
            _channel.AddBinders(_handlerCatalog.Binders);
            _channel.OnMessageReceived += ProcessMessageReceived;
        }
        
        public void Publish<T>(string subject, T data)
        {
            var msg = data as MessageData;
            if (msg != null)
            {
                _channel.Publish(subject, msg);
            }
            else
            {
                Publish(subject, data, _encondingTypeDefault);
            }
        }

        public void Publish<T>(string subject, T data, MessageEncodingType enconding)
        {
            Publish(subject, data, enconding, string.Empty, Guid.NewGuid().ToString(), string.Empty);
        }

        public void Publish<T>(string subject,
            T data,
            MessageEncodingType encodingType,
            string callback,
            string messageId,
            string correlationId)
        {
            var msg = CreateMessageData(data, callback, messageId, correlationId, encodingType, null);
            _channel.Publish(subject, msg);
        }

        public TReq Request<TReq>(string topic, object data, TimeSpan timeOut, string[] acceptsEnconding = null)
        {
            var msg = CreateMessageData(data, string.Empty, Guid.NewGuid().ToString(), string.Empty,
                _encondingTypeDefault, acceptsEnconding);
            var resp = Request(topic, msg, timeOut);
            return resp.DecodeMessage<TReq>();
        }

        public MessageData Request(string subject, MessageData data, TimeSpan timeOut)
        {
            var evt = new ManualResetEvent(true);
            var messageId = data.MessageId;
            if (data.Headers != null)
            {
                data.Headers[CALLBACK] = _handlerCatalog?.DefaultCallback;
            }
            Publish(subject, data);
            while (evt.WaitOne(timeOut))
            {
                if (!_callbackObj.ContainsKey(messageId) || _callbackObj[messageId] == null) continue;
                var ret = _callbackObj[messageId];
                _callbackObj.Remove(messageId);
                return ret;
            }
            throw new Exception("Request timeout");
        }

        public MessageEncodingType GetAcceptEnconding(IDictionary<string, string> headers)
        {
            var enconding = _encondingTypeDefault;
            if (headers == null || !headers.ContainsKey(ACCEPT_ENCONDING) || string.IsNullOrEmpty(headers[ACCEPT_ENCONDING]))
            {
                return enconding;
            }
            foreach (var enc in headers[ACCEPT_ENCONDING].Split(','))
            {
                if (Enum.TryParse(enc, true, out enconding))
                    break;
            }
            return enconding;
        }

        private void ProcessMessageReceived(object sender, MessageReceivedEventArgs args)
        {
            if (!string.IsNullOrEmpty(args.Data.CorrelationId) &&
                !_callbackObj.ContainsKey(args.Data.CorrelationId))
            {
                _callbackObj.Add(args.Data.CorrelationId, args.Data);
            }
            if (args.Method == null || args.Data.GetHeader(CALLBACK) == NO_REPLY)
            {
                return;
            }
            var value = GetExpectedValue(args);
            IContextHandler obj = null;
            if (_handlerCatalog.Handlers.ContainsKey(args.HandlerType.FullName))
            {
                obj = _handlerCatalog.Handlers[args.HandlerType.FullName];
            }
            var parameters = new List<object>();
            foreach (var p in args.Method.GetParameters())
            {
                if (args.Data.Headers != null && p.ParameterType.IsInstanceOfType(args.Data.Headers))
                {
                    parameters.Add(args.Data.Headers);
                }
                if (p.ParameterType == value.GetType())
                {
                    parameters.Add(value);
                }
            }
            var ret = args.Method.Invoke(obj, BindingFlags.Public, null, parameters.ToArray(),
                CultureInfo.InvariantCulture);
            if (ret != null && args.Data.GetHeader(CALLBACK) != null)
            {
                Publish(args.Data.GetHeader(CALLBACK),
                    ret,
                    GetAcceptEnconding(args.Data.Headers),
                    NO_REPLY,
                    Guid.NewGuid().ToString(),
                    args.Data.MessageId);
            }
        }

        private static MessageData CreateMessageData<T>(T data,
            string callback,
            string messageId,
            string correlationId,
            MessageEncodingType encodingType,
            IEnumerable<string> acceptEnconding)
        {
            var output = data.CodeMessage(encodingType);
            output.MessageId = messageId;
            output.CorrelationId = correlationId;
            output.Headers = new Dictionary<string, string>
            {
                {EXPECTED_ARGUMENT_TYPE, data.GetType().Name},
                {CALLBACK, callback},
                {ACCEPT_ENCONDING, acceptEnconding?.Aggregate((c, n) => $"{c},{n}")}
            };
            return output;
        }

        private static object GetExpectedValue(MessageReceivedEventArgs args)
        {
            var expectedArgumentType =
                args.ExpectedArgumentType ?? Type.GetType(args.Data.GetHeader(EXPECTED_ARGUMENT_TYPE));
            return args.Data.DecodeMessage(expectedArgumentType);
        }

        public void Dispose()
        {
            _channel?.Close();
        }
    }
}

