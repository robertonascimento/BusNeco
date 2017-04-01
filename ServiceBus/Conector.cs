
using System.Threading;

namespace ServiceBus
{
    using System.Collections.Generic;
    using System;
    using System.IO;
    using Infra.Entities;
    using Infra.Enums;
    using Infra.Interfaces;
    using System.Globalization;
    using System.Reflection;
    using Castle.MicroKernel.Registration;
    using Castle.Windsor;
    
    public class Conector : IConector
    {
        private readonly IWindsorContainer _container;
        private readonly ConectorSettings _settings;
        private readonly IModuleCatalog _moduleCatalog;
        private IList<IChannel> _channels;
        private readonly IDictionary<string, object> _callbackObj;
        
        public Conector(IWindsorContainer container) 
            : this(container, new ConectorSettings {EncodingType = MessageEncodingType.Json})
        {
        }

        public Conector(IWindsorContainer container, string moduleName)
            : this(container, new ConectorSettings { EncodingType = MessageEncodingType.Json }, moduleName)
        {
        }

        public Conector(IWindsorContainer container, ConectorSettings settings)
        {
            _container = container;
            _settings = settings;
            _moduleCatalog = new ModuleCatalog();
            _callbackObj = new Dictionary<string, object>();
        }

        public Conector(IWindsorContainer container, ConectorSettings settings, string moduleName)
        {
            _container = container;
            _settings = settings;
            _moduleCatalog = new ModuleCatalog(moduleName);
            _callbackObj = new Dictionary<string, object>();
        }

        public void SetUp()
        {
            var path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var classes = Classes.FromAssemblyInDirectory(new AssemblyFilter(path,"*"));
            _moduleCatalog.AddAssembly(Assembly.GetCallingAssembly());
            _container.Register(Component.For<IModuleCatalog>().Instance(_moduleCatalog));
            _container.Register(classes.BasedOn<IChannel>().WithServiceAllInterfaces().LifestyleSingleton());
            _container.Register(classes.BasedOn(_moduleCatalog.HandlersType).WithServiceAllInterfaces().LifestyleSingleton());
            _channels = _container.ResolveAll<IChannel>();
            foreach (var channel in _channels)
            {
                channel.SetUp();
                channel.MessageReceived += MessageReceived;
            }
        }

        public T Request<T>(string topic, object data, TimeSpan timeOut)
        {
            var evt = new ManualResetEvent(true);
            var correlation = Guid.NewGuid().ToString();
            Publish(topic, data, $"{_moduleCatalog.ModuleName}.callback", correlation);
            while (evt.WaitOne(timeOut))
            {
                if (_callbackObj.ContainsKey(correlation) && _callbackObj[correlation] is T)
                {
                    var ret = (T)_callbackObj[correlation];
                    _callbackObj.Remove(correlation);
                    return ret;
                }
            }
            throw new Exception("Request timeout");
        }

        public void Publish(string topic, object data)
        {
            Publish(topic, data, string.Empty, Guid.NewGuid().ToString());
        }

        private void Publish(string topic, object data, string replyTo, string correlationId)
        {
            IMessageData output = null;
            if (_settings.EncodingType == MessageEncodingType.Json)
            {
                output = data.ToJsonEncode();
                output.ReplyTo = replyTo;
                output.CorrelationId = correlationId;
                output.Headers = new Dictionary<string, object> {{"ExpectedArgumentType", data.GetType()}};
            }
            foreach (var channel in _channels)
            {
                channel.Publish(topic, output);
            }
        }
        
        private void MessageReceived(object sender, MessageReceivedEventArgs args)
        {
            var expectedArgumentType = args.ExpectedArgumentType;
            if (expectedArgumentType == null && args.Data.Headers.ContainsKey("ExpectedArgumentType"))
            {
                expectedArgumentType = Type.GetType((string)args.Data.Headers["ExpectedArgumentType"]);
            }
            var value = DecodeMessage(args.Data, expectedArgumentType);
            if (args.Method == null)
            {
                _callbackObj.Add(args.Data.CorrelationId, value);
                return;
            }
            var obj = _container.Resolve(args.HandlerType);
            var ret = args.Method?.Invoke(obj, BindingFlags.Public, null, new[] { value }, CultureInfo.InvariantCulture);
            if (ret != null)
            {
                Publish(args.Data.ReplyTo, ret, "", args.Data.CorrelationId);
            }
        }

        private static object DecodeMessage(IMessageData data, Type expected)
        {
            object value = null;
            if (expected != null &&
                !string.IsNullOrEmpty(data.ContentType))
            {
                if (data.ContentType.ToLowerInvariant() == "application/json")
                {
                    value = data.FromJsonEncode(expected);
                }
            }
            else
            {
                value = data.Body;
            }
            return value;
        }
    }
}

