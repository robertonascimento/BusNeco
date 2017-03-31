namespace ServiceBus.Infra.Attributes
{
    using System;
    using Interfaces;

    public class RespondAttribute : Attribute, IMemberInfo
    {
        public string Name { get; set; }

        public RespondAttribute(string topicName)
        {
            Name = topicName;
        }
    }
}
