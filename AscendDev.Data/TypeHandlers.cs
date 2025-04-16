using System.Data;
using System.Text.Json;
using Dapper;
using Npgsql;
using NpgsqlTypes;

namespace AscendDev.Data;

public class JsonTypeHandler<T>(JsonSerializerOptions? options = null) : SqlMapper.TypeHandler<T>
{
    private readonly JsonSerializerOptions _options = options ?? new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public override T Parse(object value)
    {
        if (value is string json)
            return JsonSerializer.Deserialize<T>(json, _options) ??
                   throw new InvalidOperationException("Deserialized JSON is null");

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