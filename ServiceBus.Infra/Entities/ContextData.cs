using ServiceBus.Infra.Interfaces;

namespace ServiceBus.Infra.Entities
{
    public class BusMessageContext<T> : IBusMessageContext
    {
        public MessageData Data { get; set; }
        public IConector Conector { get; set; }

        public string GetString() => Data.DecodeMessage<string>();

        public T GetObject() => (T)Data.DecodeMessage(typeof(T));
    }
}
