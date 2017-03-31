using System;

namespace TestBus
{
    public class Program
    {
        protected Program()
        {
            
        }

        public static void Main()
        {
            var t = new Tester();
            t.Go();
            Console.ReadKey();
        }
    }
}
