using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Processes.Operaton.WebApi.Operaton.Generated;

public partial class AnyValue
{
    [JsonIgnore]
    public JsonElement JsonValue { get; init; }
}

public partial class OperatonApiClient
{
    static partial void UpdateJsonSerializerSettings(JsonSerializerOptions settings)
    {
        settings.Converters.Add(new AnyValueJsonConverter());
        settings.Converters.Add(new OperatonDateTimeOffsetJsonConverter());
    }
}

internal sealed class AnyValueJsonConverter : JsonConverter<AnyValue>
{
    public override AnyValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        return new AnyValue { JsonValue = document.RootElement.Clone() };
    }

    public override void Write(Utf8JsonWriter writer, AnyValue value, JsonSerializerOptions options)
    {
        if (value.JsonValue.ValueKind == JsonValueKind.Undefined)
        {
            writer.WriteNullValue();
            return;
        }

        value.JsonValue.WriteTo(writer);
    }
}

internal sealed class OperatonDateTimeOffsetJsonConverter : JsonConverter<DateTimeOffset>
{
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString() ?? throw new JsonException("Operaton date value is null.");
        var normalizedValue = HasCompactOffset(value)
            ? value.Insert(value.Length - 2, ":")
            : value;

        if (DateTimeOffset.TryParse(
                normalizedValue,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var result))
        {
            return result;
        }

        throw new JsonException($"'{value}' is not a supported Operaton date format.");
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        var formattedValue = value.ToString("yyyy-MM-dd'T'HH:mm:ss.fffzzz", CultureInfo.InvariantCulture);
        writer.WriteStringValue(formattedValue.Remove(formattedValue.Length - 3, 1));
    }

    private static bool HasCompactOffset(string value) =>
        value.Length >= 5
        && value[^5] is '+' or '-'
        && char.IsDigit(value[^4])
        && char.IsDigit(value[^3])
        && char.IsDigit(value[^2])
        && char.IsDigit(value[^1]);
}
