using ServiceBus;
using ServiceBus.Channel.FileBroker;
using ServiceBus.Channel.RabbitMq;
using ServiceBus.Infra.Entities;
using ServiceBus.Infra.Enums;
using ServiceBus.Infra.Interfaces;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using TestBus.Test;

namespace TestBus
{
    public class Program
    {
                
        protected Program()
        {            
        }

        public static void Main()
        {
            TestUsingFile();
            // TestUsingRabbitMQ();
        }

        private static void TestUsingFile()
        {
            var catalog = new HandlerCatalog().AddInstance(new TradeManager()).Load();
            var conector = new Conector(new FileChannel(catalog), MessageEncodingType.Json);
            conector.SetUp(catalog);
            RunTest(conector);
        }
        
        private static void TestUsingRabbitMQ()
        {
            //Make sure your RabbitMQ service is up and runing and check Config\RabbitChannelConfig.json configuration
            var catalog = new HandlerCatalog().AddInstance(new TradeManager()).Load();
            var conector = new Conector(new RabbitMqChannel(), MessageEncodingType.Json);
            conector.SetUp(catalog);
            RunTest(conector);           
        }

        private static void RunTest(IConector conector)
        {            
            //Request Response Test
            var sw = Stopwatch.StartNew();
            Console.WriteLine(conector.Request<bool>("module1.create", new Trade { TradeDate = DateTime.Today, Account = new Random().Next(10000, 20000) }, TimeSpan.FromSeconds(3)));
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);

            //Publish Test
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    conector.Publish("module1.capture", new Trade { TradeDate = DateTime.Today, Account = new Random().Next(10000, 20000) });
                    Thread.Sleep(5000);
                }
            });
            Console.ReadKey();
        }
    }
}
