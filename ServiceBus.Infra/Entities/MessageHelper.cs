namespace ServiceBus.Infra.Entities {
    using System;
    using System.Text;
    using Enums;
    using Newtonsoft.Json;
    using ZeroFormatter;

    public static class MessageHelper {

        public static MessageData ToZeroFormatterEncode<T>(this T data) => new MessageData
        {
            ContentType = "application/zeroformatter",
            Body = ZeroFormatterSerializer.Serialize(data)
        };

        public static MessageData ToZeroFormatterLz4Encode<T>(this T data) => new MessageData
        {
            ContentType = "application/zeroformatterlz4",
            Body = LZ4.LZ4Codec.Wrap(ZeroFormatterSerializer.Serialize(data))
        };

        public static MessageData ToJsonEncode(this object data) => new MessageData
        {
            ContentType = "application/json",
            Body = Encoding.UTF8.GetBytes(data.ToJson())
        };

        public static MessageData ToJsonLz4Encode(this object data) => new MessageData
        {
            ContentType = "application/jsonlz4",
            Body = LZ4.LZ4Codec.Wrap(Encoding.UTF8.GetBytes(data.ToJson()))
        };

        public static object FromZeroFormatterEncode(this MessageData data, Type type) => ZeroFormatterSerializer.NonGeneric.Deserialize(type, data.Body);

        public static object FromZeroFormatterLz4Encode(this MessageData data, Type type) => ZeroFormatterSerializer.NonGeneric.Deserialize(type, LZ4.LZ4Codec.Unwrap(data.Body));

        public static object FromJsonEncode(this MessageData data, Type type) {
            var message = Encoding.UTF8.GetString(data.Body);
            return message.FromJson(type);
        }

        public static object FromJsonLz4Encode(this MessageData data, Type type) {
            var message = Encoding.UTF8.GetString(LZ4.LZ4Codec.Unwrap(data.Body));
            return message.FromJson(type);
        }

        public static string ToJson(this object data, bool ignoreNull = true) 
            => JsonConvert.SerializeObject(data, Formatting.None,
                        new JsonSerializerSettings
                        {
                            NullValueHandling = ignoreNull ? NullValueHandling.Ignore : NullValueHandling.Include
                        });

        public static object FromJson(this string data, Type type) => JsonConvert.DeserializeObject(data, type);

        public static T FromJson<T>(this string data) => JsonConvert.DeserializeObject<T>(data);

        public static T FromJsonFile<T>(this string fileName) => JsonConvert.DeserializeObject<T>(System.IO.File.ReadAllText(fileName));

        public static MessageData CodeMessage<T>(this T data,  MessageEncodingType encodingType) {
            
            switch (encodingType) {
                case MessageEncodingType.Json:
                    return data.ToJsonEncode();
                case MessageEncodingType.JsonLz4:
                    return data.ToJsonLz4Encode();
                case MessageEncodingType.ZeroFormatterLz4:
                    return data.ToZeroFormatterLz4Encode();
                case MessageEncodingType.ZeroFormatter:
                    return data.ToZeroFormatterEncode();
            }
            return new MessageData
            {
                Body = (byte[])Convert.ChangeType(data, typeof(byte[]))
            };
        }

        public static T DecodeMessage<T>(this MessageData data) => (T)DecodeMessage(data, typeof(T));

        public static object DecodeMessage(this MessageData data, Type expected) 
        {           
            if (expected == null || string.IsNullOrEmpty(data.ContentType))
                return data.Body;
            
                switch (data.ContentType.ToLowerInvariant()) {
                    case "application/json":
                        if (expected == typeof(string)) {
                            return Encoding.UTF8.GetString(data.Body);
                        }
                        return data.FromJsonEncode(expected);
                        
                    case "application/jsonlz4":
                        if (expected == typeof(string))
                        {
                            return Encoding.UTF8.GetString(LZ4.LZ4Codec.Unwrap(data.Body));
                        }
                        return data.FromJsonLz4Encode(expected);
                        
                    case "application/zeroformatter":
                        return data.FromZeroFormatterEncode(expected);
                        
                    case "application/zeroformatterlz4":
                        return data.FromZeroFormatterLz4Encode(expected);
                        
                }

            return data.Body;
        }
    }
}
