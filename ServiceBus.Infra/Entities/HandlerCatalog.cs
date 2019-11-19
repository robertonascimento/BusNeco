namespace ServiceBus.Infra.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using Attributes;
    using Interfaces;

    public class HandlerCatalog
    {
        public string DefaultCallback { get; set; }
        public IDictionary<string, MethodMetadata> Binders { get; set; }
        public Dictionary<string, IContextHandler> Handlers { get; set; }

        public HandlerCatalog()
        {
            Binders = new Dictionary<string, MethodMetadata>();
            Handlers = new Dictionary<string, IContextHandler>();
        }

        public HandlerCatalog AddInstance(params IContextHandler[] context)
        {
            if (context == null) return this;
            foreach (var ctx in context)
            {
                var type = ctx.GetType();
                if (Attribute.IsDefined(type, typeof(HandlerAttribute)))
                {
                    Handlers.Add(type.FullName, ctx);
                }
            }            
            return this;
        }

        public HandlerCatalog Load()
        {
            foreach (var type in Handlers.Values.Select(it=>it.GetType()))
            {
                var handler =
                    type.GetCustomAttributes(false)
                        .FirstOrDefault(f => f.GetType() == typeof(HandlerAttribute)) as IMemberInfo;
                if (handler == null)
                {
                    continue;
                }

                foreach (var method in type.GetMethods())
                {
                    foreach (var attr in method.GetCustomAttributes(false))
                    {
                        var topic = attr as IMemberInfo;
                        if (topic == null)
                        {
                            continue;
                        }
                        topic.Name = topic.Name.ToLower(CultureInfo.InvariantCulture).Replace("@", handler.Name);
                        var param = method.GetParameters().FirstOrDefault()?.ParameterType;
                        if (param != null && (!param.IsGenericType || param.GetGenericTypeDefinition() != typeof(BusMessageContext<>)))
                            throw new Exception($"The first method parameter should be a type of {nameof(IBusMessageContext)}");

                        var methodMetadata = new MethodMetadata
                        {
                            HandlerInfo = handler,
                            HandlerType = type,
                            Method = method,
                            MethodInfo = topic,
                            ExpectedArgumentType = param.GetGenericArguments().FirstOrDefault()
                        };
                        if (attr.GetType() == typeof(ListenAttribute) || attr.GetType() == typeof(RespondAttribute))
                        {
                            AddBinder(topic.Name, methodMetadata);
                        }
                    }
                }
                DefaultCallback = $"{handler.Name}.callback";
                AddBinder(DefaultCallback, new MethodMetadata());
            }
            return this;
        }

        private void AddBinder(string key, MethodMetadata value)
        {
            if (!Binders.ContainsKey(key))
            {
                Binders.Add(key, value);
            }
        }
    }
}