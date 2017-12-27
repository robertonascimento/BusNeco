namespace ServiceBus.Infra.Interfaces
{
    using System;
    using System.Collections.Generic;
    using Entities;

    public interface IChannel
    {
        string ChannelId { get; }
        void SetUp();
        void Close();
        event EventHandler<MessageReceivedEventArgs> OnMessageReceived;
        void Publish(string subject, MessageData data);
        MessageData Request(string subject, MessageData data, TimeSpan timeOut);
        void AddBinders(IDictionary<string, MethodMetadata> binders);
    }
}
