namespace ServiceBus.Infra.Entities {
    using System;
    using System.Text;
    using Enums;
    using Newtonsoft.Json;
    using ZeroFormatter;

    public static class JsonHelper {
        public static MessageData ToZeroFormatterEncode<T>(this T data) {
            var ret = new MessageData {
                ContentType = "application/zeroformatter",
                Body = ZeroFormatterSerializer.Serialize(data)
            };
            return ret;
        }

        public static MessageData ToZeroFormatterLz4Encode<T>(this T data) {
            var ret = new MessageData {
                ContentType = "application/zeroformatterlz4",
                Body = LZ4.LZ4Codec.Wrap(ZeroFormatterSerializer.Serialize(data))
            };
            return ret;
        }

        public static MessageData ToJsonEncode(this object data) {
            var ret = new MessageData {
                ContentType = "application/json",
                Body = Encoding.UTF8.GetBytes(data.ToJson())
            };
            return ret;
        }

        public static MessageData ToJsonLz4Encode(this object data) {
            var ret = new MessageData {
                ContentType = "application/jsonlz4",
                Body = LZ4.LZ4Codec.Wrap(Encoding.UTF8.GetBytes(data.ToJson()))
            };
            return ret;
        }

        public static object FromZeroFormatterEncode(this MessageData data, Type type) {
            return ZeroFormatterSerializer.NonGeneric.Deserialize(type, data.Body);
        }

        public static object FromZeroFormatterLz4Encode(this MessageData data, Type type) {
            return ZeroFormatterSerializer.NonGeneric.Deserialize(type, LZ4.LZ4Codec.Unwrap(data.Body));
        }

        public static object FromJsonEncode(this MessageData data, Type type) {
            var message = Encoding.UTF8.GetString(data.Body);
            return message.FromJson(type);
        }

        public static object FromJsonLz4Encode(this MessageData data, Type type) {
            var message = Encoding.UTF8.GetString(LZ4.LZ4Codec.Unwrap(data.Body));
            return message.FromJson(type);
        }

        public static string ToJson(this object data, bool ignoreNull = true) {
            return JsonConvert.SerializeObject(data,
                Formatting.None,
                new JsonSerializerSettings {
                    NullValueHandling = ignoreNull ? NullValueHandling.Ignore : NullValueHandling.Include
                });
        }

        public static object FromJson(this string data, Type type) {
            return JsonConvert.DeserializeObject(data, type);
        }

        public static T FromJson<T>(this string data) {
            return JsonConvert.DeserializeObject<T>(data);
        }

        public static T FromJsonFile<T>(this string fileName) {
            return JsonConvert.DeserializeObject<T>(System.IO.File.ReadAllText(fileName));
        }

        public static MessageData CodeMessage<T>(this T data,
            MessageEncodingType encodingType) {
            MessageData output;
            switch (encodingType) {
                case MessageEncodingType.Json:
                    output = data.ToJsonEncode();
                    break;
                case MessageEncodingType.JsonLz4:
                    output = data.ToJsonLz4Encode();
                    break;
                case MessageEncodingType.ZeroFormatterLz4:
                    output = data.ToZeroFormatterLz4Encode();
                    break;
                case MessageEncodingType.ZeroFormatter:
                    output = data.ToZeroFormatterEncode();
                    break;
                default:
                    output = new MessageData {
                        Body = (byte[]) Convert.ChangeType(data, typeof(byte[]))
                    };
                    break;
            }
            return output;
        }

        public static T DecodeMessage<T>(this MessageData data) {
            return (T) DecodeMessage(data, typeof(T));
        }

        public static object DecodeMessage(this MessageData data, Type expected) {
            object value = null;
            if (expected != null &&
                !string.IsNullOrEmpty(data.ContentType)) {
                switch (data.ContentType.ToLowerInvariant()) {
                    case "application/json":
                        if (expected == typeof(string)) {
                            return Encoding.UTF8.GetString(data.Body);
                        }
                        value = data.FromJsonEncode(expected);
                        break;
                    case "application/jsonlz4":
                        value = data.FromJsonLz4Encode(expected);
                        break;
                    case "application/zeroformatter":
                        data.FromZeroFormatterEncode(expected);
                        break;
                    case "application/zeroformatterlz4":
                        data.FromZeroFormatterLz4Encode(expected);
                        break;
                }
            }
            else {
                value = data.Body;
            }
            return value;
        }
    }
}
