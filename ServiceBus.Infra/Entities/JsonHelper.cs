namespace ServiceBus.Infra.Entities
{
    using System;
    using System.Text;
    using Newtonsoft.Json;
    using Interfaces;

    public static class JsonHelper
    {
        public static IMessageData ToJsonEncode(this object data) 
        {
            var ret = new MessageData
            {
                ContentType = "application/json",
                Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data))
            };
            return ret;
        }

        public static object FromJsonEncode(this IMessageData data, Type type)
        {
            var message = Encoding.UTF8.GetString(data.Body);
            var ret = JsonConvert.DeserializeObject(message, type);
            return ret;
        }

        public static string ToJson(this object data)
        {
            return JsonConvert.SerializeObject(data);
        }

        public static object FromJson(this string data)
        {
            return JsonConvert.DeserializeObject(data);
        }

        public static T FromJson<T>(this string data)
        {
            return JsonConvert.DeserializeObject<T>(data);
        }

        public static T FromJsonFile<T>(this string fileName)
        {
            return JsonConvert.DeserializeObject<T>(System.IO.File.ReadAllText(fileName));
        }
    }
}
