namespace ServiceBus.Infra.Entities
{
    using System.Collections.Generic;
    

    public class MessageData 
    {
        public byte[] Body { get; set; }
        public string ContentEncoding { get; set; }
        public string ContentType { get; set; }
        public string CorrelationId { get; set; }
        public byte DeliveryMode { get; set; }
        public string Expiration { get; set; }
        public IDictionary<string, string> Headers { get; set; }
        public string MessageId { get; set; }
        public bool Persistent { get; set; }
        public byte Priority { get; set; }
        public string ReplyTo { get; set; }
        public string Type { get; set; }
        public string UserId { get; set; }

        public string GetHeader(string key)
        {
            if (Headers != null && Headers.ContainsKey(key))
                return Headers[key];
            return null;
        }
    }
}
