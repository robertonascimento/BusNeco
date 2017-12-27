namespace ServiceBus.Infra.Entities
{
    using System;
    using System.Reflection;
    using Interfaces;

    public class MessageReceivedEventArgs : EventArgs
    {
        public IMemberInfo HandlerInfo { get; set; }
        public MessageData Data { get; set; }
        public MethodInfo Method { get; set; }
        public Type HandlerType { get; set; }
        public Type ExpectedArgumentType { get; set; }

        public static MessageReceivedEventArgs Create(MethodMetadata metadata, MessageData data)
        {
            return new MessageReceivedEventArgs
            {
                ExpectedArgumentType = metadata?.ExpectedArgumentType,
                HandlerType = metadata?.HandlerType,
                Method = metadata?.Method,
                HandlerInfo = metadata?.HandlerInfo,
                Data = data
            };
        }
    }
}
