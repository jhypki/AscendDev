using ElearningPlatform.Core.Interfaces.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace ElearningPlatform.Data;

public class PostgresqlConnectionManager(IConfiguration configuration) : IConnectionManager<NpgsqlConnection>
{
    private readonly string _connectionString = configuration.GetConnectionString("Postgres")
                                                ?? throw new ArgumentNullException(nameof(configuration),
                                                    "Postgres connection string is missing in configuration");

    public virtual NpgsqlConnection GetConnection()
    {
        try
        {
            var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            return connection;
        }
        catch (Exception ex)
        {
            throw new Exception("Error connecting to database", ex);
        }
    }
}