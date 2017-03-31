namespace ServiceBus.Infra.Attributes
{
    using System;
    using Interfaces;

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple = true)]
    public class HandlerAttribute : Attribute, IHandlerInfo
    {
        public string[] Consumers { get; set; }

        public HandlerAttribute()
        {
        }

        public HandlerAttribute(string[] consumers)
        {
            Consumers = consumers;
        }
        
    }
}
