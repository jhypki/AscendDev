using System.Data;
using System.Text.Json;
using Dapper;
using Npgsql;
using NpgsqlTypes;
using AscendDev.Core.JsonConverters;
using AscendDev.Core.Models.Courses;

namespace AscendDev.Data;

public class JsonTypeHandler<T>(JsonSerializerOptions? options = null) : SqlMapper.TypeHandler<T>
{
    private readonly JsonSerializerOptions _options = options ?? GetDefaultOptions();

    private static JsonSerializerOptions GetDefaultOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        // Add the TestCaseConverter to handle TestCase objects properly
        options.Converters.Add(new TestCaseConverter());

        return options;
    }

    public override T Parse(object value)
    {
        // Add debugging to understand what we're receiving
        Console.WriteLine($"JsonTypeHandler.Parse: Received value of type {value?.GetType()?.Name ?? "null"} for target type {typeof(T).Name}");

        if (value is string json)
        {
            Console.WriteLine($"JsonTypeHandler.Parse: Processing as string: {json.Substring(0, Math.Min(100, json.Length))}...");
            return JsonSerializer.Deserialize<T>(json, _options) ??
                   throw new InvalidOperationException("Deserialized JSON is null");
        }

        if (value is JsonElement jsonElement)
        {
            var jsonString = jsonElement.GetRawText();
            Console.WriteLine($"JsonTypeHandler.Parse: Processing JsonElement as string: {jsonString.Substring(0, Math.Min(100, jsonString.Length))}...");
            return JsonSerializer.Deserialize<T>(jsonString, _options) ??
                   throw new InvalidOperationException("Deserialized JSON is null");
        }

        Console.WriteLine($"JsonTypeHandler.Parse: Unable to handle value type {value?.GetType()?.FullName ?? "null"}");
        throw new InvalidCastException($"Unable to cast {value.GetType()} to {typeof(T)}");
    }

    public override void SetValue(IDbDataParameter parameter, T value)
    {
        if (parameter is NpgsqlParameter npgsqlParameter)
        {
            npgsqlParameter.NpgsqlDbType = NpgsqlDbType.Jsonb;
            npgsqlParameter.Value = JsonSerializer.Serialize(value, _options);
        }
    }
}

public class JsonDocumentTypeHandler : SqlMapper.TypeHandler<JsonDocument?>
{
    public override JsonDocument? Parse(object value)
    {
        if (value == null || value is DBNull)
            return null;

        if (value is string json)
        {
            return JsonDocument.Parse(json);
        }

        throw new InvalidCastException($"Unable to cast {value.GetType()} to JsonDocument");
    }

    public override void SetValue(IDbDataParameter parameter, JsonDocument? value)
    {
        if (parameter is NpgsqlParameter npgsqlParameter)
        {
            npgsqlParameter.NpgsqlDbType = NpgsqlDbType.Jsonb;
            npgsqlParameter.Value = value?.RootElement.GetRawText() ?? (object)DBNull.Value;
        }
    }
}