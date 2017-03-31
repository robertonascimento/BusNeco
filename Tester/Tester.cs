using System.Threading;
using System.Threading.Tasks;

namespace TestBus
{
    using System;
    using Castle.MicroKernel.Registration;
    using Castle.Windsor;
    using ServiceBus;
    using ServiceBus.Infra.Interfaces;
    using Test;

    public class Tester
    {
        private readonly IWindsorContainer _container;
        private IConector _conector;
        private string _moduleName = "module1";

        public Tester()
        {
            _container = new WindsorContainer();
        }

        public void Go()
        {
            Console.WriteLine(_moduleName);
            _container.Register(Component.For<IWindsorContainer>().Instance(_container));
            _conector = new Conector(_container, _moduleName);
            _conector.SetUp();


            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    _conector.Publish("module2.capture", new Trade { TradeDate = DateTime.Today, Account = new Random().Next(10000, 20000) });
                    Thread.Sleep(1500);
                }
            });
            Console.ReadKey();
        }
        
    }
}
