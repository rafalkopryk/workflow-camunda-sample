using Confluent.Kafka;
using MassTransit.KafkaIntegration.Serializers;
using System.Net.Mime;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Processes.Application.Extensions;

public class JsonKafkaSerializerFactory : IKafkaSerializerFactory
{
    public ContentType ContentType => new("application/json");

    public static readonly JsonSerializerOptions Options = GetJsonSerializerOptions();

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

    public IDeserializer<T> GetDeserializer<T>()
    {
        return new MassTransitJsonDeserializer<T>();
    }

    public IAsyncSerializer<T> GetSerializer<T>()
    {
        return new KafkaAsyncJsonSerializer<T>();
    }

    public class KafkaJsonSerializer<T> : ISerializer<T>
    {
        public byte[] Serialize(T data, SerializationContext context)
        {
            return JsonSerializer.SerializeToUtf8Bytes(data, Options);
        }
    }

    public class KafkaAsyncJsonSerializer<T> : IAsyncSerializer<T>
    {
        public Task<byte[]> SerializeAsync(T data, SerializationContext context)
        {
            return Task.FromResult(JsonSerializer.SerializeToUtf8Bytes(data, Options));
        }
    }

    public class KafkaJsonDeserializer<T> : IDeserializer<T>
    {
        public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            if (data.IsEmpty && isNull)
                return default;

            return JsonSerializer.Deserialize<T>(data, Options);
        }
    }
}

