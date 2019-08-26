using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace ThunderHawk.Core
{
    public static class JsonHelper
    {
        public static JsonSerializer Serializer { get; }
        public static JsonSerializerSettings SerializerSettings { get; }

        static JsonHelper()
        {

            SerializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            SerializerSettings.Converters.Add(new StringEnumConverter());
            Serializer = JsonSerializer.Create(SerializerSettings);
        }
    }
}
