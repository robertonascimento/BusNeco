
using System;
using ServiceBus.Infra.Interfaces;

namespace ServiceBus.Infra.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ListenAttribute : Attribute, IMemberInfo
    {
        public string Name { get; set; }

        public ListenAttribute(string topicName)
        {
            Name = topicName;
        }
    }
}
