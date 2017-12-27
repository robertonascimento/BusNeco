namespace ServiceBus.Channel.RabbitMq
{
    public class Config
    {
        public bool DeclareExchange { get; set; }
        public string ExchangeType { get; set; }
        public string ExchangeName { get; set; }
        public bool DeclareQueue { get; set; }
        public string QueueName { get; set; }
        public string ServerName { get; set; }
        public bool ExchangeDurable { get; set; }
        public bool ExchangeAutoDelete { get; set; }
        public bool QueueDurable { get; set; }
        public bool QueueAutoDelete { get; set; }
        public string UserLogin { get; set; }
        public string Password { get; set; }
        public int? Port { get; set; }
        public string VirtualHost { get; set; }
    }
}
