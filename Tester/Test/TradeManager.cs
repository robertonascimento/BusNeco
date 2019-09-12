using ServiceBus.Infra.Entities;
using System;
using ServiceBus.Infra.Attributes;

namespace TestBus.Test
{
    [Handler("module1")]
    public class TradeManager : IContextHandler
    {
        [Respond("@.create")]
        public bool Create(Trade trade)
        {
            return true;
        }

        [Listen("module1.capture")]
        public void Capture(Trade trade)
        {
            Console.WriteLine(trade);
        }
    }
}