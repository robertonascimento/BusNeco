namespace ServiceBus.Infra.Entities
{
    using System;
    using System.Reflection;
    using Interfaces;

    public class MethodMetadata
    {
        public IHandlerInfo HandlerInfo { get; set; }
        public Type HandlerType { get; set; }
        public IMemberInfo MethodInfo { get; set; }
        public MethodInfo Method { get; set; }
        public Type ExpectedArgumentType { get; set; }
    }
}