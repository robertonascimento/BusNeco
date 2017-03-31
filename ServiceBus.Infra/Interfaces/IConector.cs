namespace ServiceBus.Infra.Interfaces
{
    public interface IConector
    {
        void SetUp();
        void Publish(string topic, object data);
    }
}