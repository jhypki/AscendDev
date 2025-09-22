using System.Text.Json;
using System.Text.Json.Serialization;
using AscendDev.Core.Models.Courses;

namespace AscendDev.Core.JsonConverters;

public class TestCaseConverter : JsonConverter<TestCase>
{
    public override TestCase Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        var testCase = new TestCase();

        if (root.TryGetProperty("name", out var nameElement))
        {
            testCase.Name = nameElement.GetString();
        }

        if (root.TryGetProperty("description", out var descElement))
        {
            testCase.Description = descElement.GetString();
        }

        if (root.TryGetProperty("input", out var inputElement))
        {
            testCase.Input = ConvertJsonElementToObject(inputElement);
        }

        if (root.TryGetProperty("expectedOutput", out var outputElement))
        {
            testCase.ExpectedOutput = ConvertJsonElementToObject(outputElement);
        }

        return testCase;
    }

    public override void Write(Utf8JsonWriter writer, TestCase value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        if (value.Name != null)
        {
            writer.WriteString("name", value.Name);
        }

        if (value.Description != null)
        {
            writer.WriteString("description", value.Description);
        }

        writer.WritePropertyName("input");
        WriteObjectValue(writer, value.Input, options);

        writer.WritePropertyName("expectedOutput");
        WriteObjectValue(writer, value.ExpectedOutput, options);

        writer.WriteEndObject();
    }

    private static object ConvertJsonElementToObject(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString()!,
            JsonValueKind.Number => element.TryGetInt32(out var intVal) ? intVal :
                                   element.TryGetInt64(out var longVal) ? longVal :
                                   element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Array => element.EnumerateArray().Select(ConvertJsonElementToObject).ToArray(),
            JsonValueKind.Object => element.EnumerateObject().ToDictionary(
                prop => prop.Name,
                prop => ConvertJsonElementToObject(prop.Value)),
            JsonValueKind.Null => null!,
            _ => element.Clone()
        };
    }

    private static void WriteObjectValue(Utf8JsonWriter writer, object? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        switch (value)
        {
            case string str:
                writer.WriteStringValue(str);
                break;
            case int intVal:
                writer.WriteNumberValue(intVal);
                break;
            case long longVal:
                writer.WriteNumberValue(longVal);
                break;
            case double doubleVal:
                writer.WriteNumberValue(doubleVal);
                break;
            case float floatVal:
                writer.WriteNumberValue(floatVal);
                break;
            case bool boolVal:
                writer.WriteBooleanValue(boolVal);
                break;
            case Array array:
                writer.WriteStartArray();
                foreach (var item in array)
                {
                    WriteObjectValue(writer, item, options);
                }
                writer.WriteEndArray();
                break;
            default:
                JsonSerializer.Serialize(writer, value, options);
                break;
        }
    }
}