
namespace TestBus.Test
{
    using ServiceBus.Infra.Attributes;

    [Handler]
    public interface ITradeManager
    {
        [Respond("@.create")]
        bool Create(Trade trade);

        [Listen("module1.capture")]
        void Capture(Trade trade);
    }
}