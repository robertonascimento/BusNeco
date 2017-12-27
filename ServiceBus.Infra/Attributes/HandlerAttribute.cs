namespace ServiceBus.Infra.Attributes
{
    using System;
    using Interfaces;

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple = true)]
    public class HandlerAttribute : Attribute, IMemberInfo
    {
        public string Name { get; set; }

        public HandlerAttribute(string name)
        {
            Name = name;
        }

    }
}
