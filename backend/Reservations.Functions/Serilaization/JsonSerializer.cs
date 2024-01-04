using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Reservations.Functions.Serilaization
{
    public interface IJsonSerializer
    {
        string Serialize<T>(T obj);
    }

    public class JsonSerializer : IJsonSerializer
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public JsonSerializer()
        {
            var contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };
            _jsonSerializerSettings = new JsonSerializerSettings()
            {
                ContractResolver = contractResolver
            };
        }

        public string Serialize<T>(T obj)
        {
            var json = JsonConvert.SerializeObject(obj, _jsonSerializerSettings);
            return json;
        }
    }
}