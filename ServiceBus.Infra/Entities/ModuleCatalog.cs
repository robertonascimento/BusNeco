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
        public List<Type> HandlersType { get; set; }

        public HandlerCatalog()
        {
            Binders = new Dictionary<string, MethodMetadata>();
            HandlersType = new List<Type>();
        }

        /// <summary>
        /// Add the assembly with Handlers on catalog
        /// </summary>
        /// <param name="assembly">The assembly</param>
        public HandlerCatalog AddAssembly(Assembly assembly)
        {
            if (assembly == null) return this;
            var types = QueryHandler(assembly.GetTypes());
            HandlersType.AddRange(types);
            return this;
        }

        public HandlerCatalog AddInstance(params IContextHandler[] context)
        {
            if (context == null) return this;
            var types = QueryHandler(context.Select(it => it.GetType()));
            HandlersType.AddRange(types);
            return this;
        }

        public void Load()
        {
            foreach (var type in HandlersType)
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
                        var methodMetadata = new MethodMetadata
                        {
                            HandlerInfo = handler,
                            HandlerType = type,
                            Method = method,
                            MethodInfo = topic,
                            ExpectedArgumentType = method.GetParameters().FirstOrDefault()?.ParameterType
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
        }

        private IEnumerable<Type> QueryHandler(IEnumerable<Type> types)
        {
            return from type in types
                   where Attribute.IsDefined(type, typeof(HandlerAttribute))
                   select type;
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