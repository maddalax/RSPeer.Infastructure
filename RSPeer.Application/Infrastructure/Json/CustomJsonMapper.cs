using System.Text.Json;
using Jose;

namespace RSPeer.Application.Infrastructure.Json
{
    public class CustomJsonMapper : IJsonMapper
    {
        public string Serialize(object obj)
        {
            return JsonSerializer.Serialize(obj);
        }

        public T Parse<T>(string json)
        {
           return JsonSerializer.Deserialize<T>(json);
        }
    }
}