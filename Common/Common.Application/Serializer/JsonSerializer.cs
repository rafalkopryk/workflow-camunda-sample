using System.Text.Json.Serialization;
using System.Text.Json;

namespace Common.Application.Serializer
{
    public static class JsonSerializerCustomOptions
    {
        public static readonly JsonSerializerOptions CamelCase = GetJsonSerializerOptions();

        private static JsonSerializerOptions GetJsonSerializerOptions()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
            };
            options.Converters.Add(new JsonStringEnumConverter());
            return options;
        }
    }
}
