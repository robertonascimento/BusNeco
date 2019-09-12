using System;

namespace ServiceBus.Infra.Interfaces
{
    using System.Collections.Generic;
    using Entities;
    using Enums;

    public interface IConector
    {
        void SetUp(HandlerCatalog catalog);
        MessageEncodingType GetAcceptEnconding(IDictionary<string, string> headers);
        void Publish<T>(string subject, T data);
        void Publish<T>(string subject, T data, MessageEncodingType enconding);

        void Publish<T>(string subject, T data, MessageEncodingType encodingType, string callback, string messageId,
            string correlationId);

        TReq Request<TReq>(string topic, object data, TimeSpan timeOut, string[] acceptsEnconding = null);

        void Dispose();
    }
}