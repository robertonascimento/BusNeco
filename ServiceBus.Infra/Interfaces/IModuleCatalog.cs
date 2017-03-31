namespace ServiceBus.Infra.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Entities;

    public interface IModuleCatalog
    {
        string ModuleName { get; set; }
        IDictionary<string, MethodMetadata> Listeners { get; set; }
        IDictionary<string, MethodMetadata> Responders { get; set; }
        List<Type> HandlersType { get; set; }
        void AddAssembly(Assembly assembly);
    }
}