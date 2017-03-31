
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
        }

        public Conector(IWindsorContainer container, ConectorSettings settings, string moduleName)
        {
            _container = container;
            _settings = settings;
            _moduleCatalog = new ModuleCatalog(moduleName);
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

        public void Publish(string topic, object data)
        {
            IMessageData output = null;
            if (_settings.EncodingType == MessageEncodingType.Json)
            {
                output = data.ToJsonEncode();
            }
            foreach (var channel in _channels)
            {
                channel.Publish(topic, output);
            }
        }

        private void MessageReceived(object sender, MessageReceivedEventArgs args)
        {
            if (args.ExpectedArgumentType == null)
            {
                throw new ArgumentNullException(nameof(args.ExpectedArgumentType));
            }

            var obj = _container.Resolve(args.HandlerType);
            var value = DecodeMessage(args.Data, args.ExpectedArgumentType);
            args.Method.Invoke(obj, BindingFlags.Public, null, new[] { value }, CultureInfo.InvariantCulture);
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

