namespace ServiceBus.Infra.Interfaces
{
    using System;
    using Entities;

    public interface IChannel
    {
        void SetUp();
        void Close();
        event EventHandler<MessageReceivedEventArgs> MessageReceived;
        void Publish(string topic, IMessageData data);
    }
}
