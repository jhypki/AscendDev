using System.Text.Json;
using System.Text.Json.Serialization;

namespace AscendDev.Core.JsonConverters;

public class DynamicValueConverter : JsonConverter<object>
{
    public override bool CanConvert(Type typeToConvert)
    {
        return true;
    }

    public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.True:
                return true;

            case JsonTokenType.False:
                return false;

            case JsonTokenType.Number:
                if (reader.TryGetInt32(out var intValue))
                    return intValue;
                if (reader.TryGetInt64(out var longValue))
                    return longValue;
                if (reader.TryGetDouble(out var doubleValue))
                    return doubleValue;
                return reader.GetDecimal();

            case JsonTokenType.String:
                return reader.GetString()!;

            case JsonTokenType.StartArray:
                using (var doc = JsonDocument.ParseValue(ref reader))
                {
                    return doc.RootElement.Clone();
                }

            case JsonTokenType.StartObject:
                using (var doc2 = JsonDocument.ParseValue(ref reader))
                {
                    return doc2.RootElement.Clone();
                }

            case JsonTokenType.Null:
                return null!;

            default:
                throw new JsonException($"Unexpected token: {reader.TokenType}");
        }
    }

    public override void Write(Utf8JsonWriter writer, object? value, JsonSerializerOptions? options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        var valueType = value.GetType();

        if (valueType == typeof(bool))
            writer.WriteBooleanValue((bool)value);
        else if (valueType == typeof(int) || valueType == typeof(long) ||
                 valueType == typeof(short) || valueType == typeof(byte))
            writer.WriteNumberValue(Convert.ToInt64(value));
        else if (valueType == typeof(float) || valueType == typeof(double) ||
                 valueType == typeof(decimal))
            writer.WriteNumberValue(Convert.ToDouble(value));
        else if (valueType == typeof(string))
            writer.WriteStringValue((string)value);
        else if (value is JsonElement element)
            element.WriteTo(writer);
        else
            // For complex objects, serialize recursively
            JsonSerializer.Serialize(writer, value, options);
    }
}