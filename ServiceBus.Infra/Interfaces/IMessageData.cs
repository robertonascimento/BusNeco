
using System.Collections.Generic;

namespace ServiceBus.Infra.Interfaces
{
    public interface IMessageData
    {
        /// <summary>The message body.</summary>
        byte[] Body { get; set; }

        /// <summary>MIME content encoding.</summary>
        string ContentEncoding { get; set; }

        /// <summary>MIME content type.</summary>
        string ContentType { get; set; }

        /// <summary>Application correlation identifier.</summary>
        string CorrelationId { get; set; }

        /// <summary>Non-persistent (1) or persistent (2).</summary>
        byte DeliveryMode { get; set; }

        /// <summary>Message expiration specification.</summary>
        string Expiration { get; set; }

        /// <summary>
        /// Message header field table. Is of type <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        IDictionary<string, object> Headers { get; set; }

        /// <summary>Application message Id.</summary>
        string MessageId { get; set; }

        /// <summary>
        /// Sets <see cref="P:RabbitMQ.Client.IBasicProperties.DeliveryMode" /> to either persistent (2) or non-persistent (1).
        /// </summary>
        bool Persistent { get; set; }

        /// <summary>Message priority, 0 to 9.</summary>
        byte Priority { get; set; }

        /// <summary>Destination to reply to.</summary>
        string ReplyTo { get; set; }

        /// <summary>Message type name.</summary>
        string Type { get; set; }

        /// <summary>User Id.</summary>
        string UserId { get; set; }

        /// <summary>
        /// Others information
        /// </summary>
        IDictionary<string, object> OthersInfo { get; set; }

    }
}
