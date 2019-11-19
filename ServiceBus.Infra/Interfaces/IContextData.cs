using ServiceBus.Infra.Entities;

namespace ServiceBus.Infra.Interfaces
{
    public interface IBusMessageContext
    {
        IConector Conector { get; set; }
        MessageData Data { get; set; }

        string GetString();
    }
}