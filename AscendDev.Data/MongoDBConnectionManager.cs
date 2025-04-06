using AscendDev.Core.Interfaces.Data;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace AscendDev.Data;

public class MongoDBConnectionManager(IConfiguration configuration) : IConnectionManager<IMongoDatabase>
{
    private readonly string _connectionString = configuration.GetConnectionString("MongoDB")
                                                ?? throw new ArgumentNullException(nameof(configuration),
                                                    "MongoDB connection string is missing in configuration");

    public virtual IMongoDatabase GetConnection()
    {
        try
        {
            var mongoClient = new MongoClient(_connectionString);
            var settings = MongoUrl.Create(_connectionString);
            var databaseName = string.IsNullOrEmpty(settings.DatabaseName)
                ? "DefaultDatabase"
                : settings.DatabaseName;

            var database = mongoClient.GetDatabase(databaseName);
            return database;
        }
        catch (Exception ex)
        {
            throw new Exception("Error connecting to MongoDB database", ex);
        }
    }
}