using System;

namespace ServiceBus.Infra.Interfaces
{
    public interface IConector
    {
        void SetUp();
        void Publish(string topic, object data);
        T Request<T>(string topic, object data, TimeSpan timeOut);
    }
}