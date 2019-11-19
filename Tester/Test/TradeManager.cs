using ServiceBus.Infra.Entities;
using System;
using ServiceBus.Infra.Attributes;

namespace TestBus.Test
{
    [Handler("module1")]
    public class TradeManager : IContextHandler
    {
        [Respond("@.create")]
        public bool Create(BusMessageContext<Trade> context)
        {
            Console.WriteLine($"CREATED {context.GetString()}");
            return true;
        }

        [Listen("module1.capture")]
        public void Capture(BusMessageContext<Trade> context)
        {
            Console.WriteLine(context.GetObject());
        }
    }
}