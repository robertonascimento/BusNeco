namespace ServiceBus.Infra.Entities
{
    using System;
    using System.Reflection;
    using Interfaces;

    public class MessageReceivedEventArgs : EventArgs
    {
        public IMessageData Data { get; set; }
        public MethodInfo Method { get; set; }
        public Type HandlerType { get; set; }
        public Type ExpectedArgumentType { get; set; }

        public static MessageReceivedEventArgs Create(MethodMetadata metadata, IMessageData data)
        {
            return new MessageReceivedEventArgs
            {
                ExpectedArgumentType = metadata?.ExpectedArgumentType,
                HandlerType = metadata?.HandlerType,
                Method = metadata?.Method,
                Data = data
            };
        }
    }
}
