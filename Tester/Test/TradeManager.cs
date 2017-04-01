using System;

namespace TestBus.Test
{
    public class TradeManager : ITradeManager
    {
        public bool Create(Trade trade)
        {
            return true;
        }

        public void Capture(Trade trade)
        {
            Console.WriteLine(trade);
        }
    }

    public class Trade
    {
        public DateTime TradeDate { get; set; }
        public int Account { get; set; }

        public override string ToString()
        {
            return $"Date: {TradeDate.ToString("yyyy-MM-dd")}, Account: {Account}";
        }
    }
}