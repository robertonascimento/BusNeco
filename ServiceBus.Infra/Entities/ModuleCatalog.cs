namespace ServiceBus.Infra.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using Attributes;
    using Interfaces;

    public class ModuleCatalog : IModuleCatalog
    {
        public string ModuleName { get; set; }
        public IDictionary<string, MethodMetadata> Listeners { get; set; }
        public IDictionary<string, MethodMetadata> Responders { get; set; }
        public List<Type> HandlersType { get; set; }

        public ModuleCatalog(string moduleName) : this()
        {
            ModuleName = moduleName;
        }

        public ModuleCatalog()
        {
            ModuleName = Assembly.GetExecutingAssembly().FullName;
            Listeners = new Dictionary<string, MethodMetadata>();
            Responders = new Dictionary<string, MethodMetadata>();
            HandlersType  = new List<Type>();
        }

        /// <summary>
        /// Add the assembly with Handlers on catalog
        /// </summary>
        /// <param name="assembly">The assembly</param>
        public void AddAssembly(Assembly assembly)
        {
            var types = (from type in assembly.GetTypes() where Attribute.IsDefined(type, typeof(HandlerAttribute)) select type).ToList();
            HandlersType.AddRange(types);
            foreach (var type in types)
            {
                var handler = type.GetCustomAttributes(false).FirstOrDefault(f => f.GetType() == typeof(HandlerAttribute)) as IHandlerInfo;
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
                        var methodMetadata = new MethodMetadata
                        {
                            HandlerInfo = handler,
                            HandlerType = type,
                            Method = method,
                            MethodInfo = topic,
                            ExpectedArgumentType = method.GetParameters().FirstOrDefault()?.ParameterType
                        };
                        if (attr.GetType() == typeof(ListenAttribute))
                        {
                            Listeners.Add(topic.Name.ToLower(CultureInfo.InvariantCulture), methodMetadata);
                        }
                        else if (attr.GetType() == typeof(RespondAttribute))
                        {
                            topic.Name = topic.Name.ToLower(CultureInfo.InvariantCulture);
                            Responders.Add(topic.Name, methodMetadata);
                        }
                    }
                }
            }
        }
    }
}