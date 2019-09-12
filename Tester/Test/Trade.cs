using System;

namespace TestBus.Test
{
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
