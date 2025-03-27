using Dapper;
using ElearningPlatform.Core.Interfaces.Data;
using Microsoft.Data.Sqlite;

namespace ElearningPlatform.Data.Test;

[TestFixture]
public class DapperSqlExecutorTests
{
    [SetUp]
    public void Setup()
    {
        _connection = new SqliteConnection(ConnectionString);
        _connection.Open();

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = """
                                          CREATE TABLE TestEntities (
                                              Id INTEGER PRIMARY KEY,
                                              Name TEXT NOT NULL
                                          )
                          """;
        cmd.ExecuteNonQuery();

        _connectionManager = new TestConnectionManager(ConnectionString);
        _sqlExecutor = new DapperSqlExecutor<SqliteConnection>(_connectionManager);
    }

    [TearDown]
    public void TearDown()
    {
        _connection?.Close();
        _connection?.Dispose();
    }

    private SqliteConnection _connection;
    private TestConnectionManager _connectionManager;
    private DapperSqlExecutor<SqliteConnection> _sqlExecutor;
    private const string ConnectionString = "Data Source=InMemorySample;Mode=Memory;Cache=Shared";

    [Test]
    public async Task QueryFirstOrDefaultAsync_ShouldReturnEntity_WhenExists()
    {
        // Arrange
        await _connection.ExecuteAsync(
            "INSERT INTO TestEntities (Id, Name) VALUES (@Id, @Name)",
            new { Id = 1, Name = "Test Entity" });

        // Act
        var result = await _sqlExecutor.QueryFirstOrDefaultAsync<TestEntity>(
            "SELECT Id, Name FROM TestEntities WHERE Id = @Id",
            new { Id = 1 });

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(1));
        Assert.That(result.Name, Is.EqualTo("Test Entity"));
    }

    [Test]
    public async Task QueryFirstOrDefaultAsync_ShouldPropagateError_WhenQueryFails()
    {
        // Act
        var exception = Assert.ThrowsAsync<SqliteException>(async () =>
            await _sqlExecutor.QueryFirstOrDefaultAsync<TestEntity>(
                "SELECT Id, Name FROM NonExistentTable WHERE Id = @Id",
                new { Id = 1 }));

        // Assert
        Assert.That(exception.Message, Does.Contain("no such table: NonExistentTable"));
    }

    [Test]
    public async Task QueryFirstOrDefaultAsync_ShouldReturnNull_WhenNotExists()
    {
        // Act
        var result = await _sqlExecutor.QueryFirstOrDefaultAsync<TestEntity>(
            "SELECT Id, Name FROM TestEntities WHERE Id = @Id",
            new { Id = 999 });

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task QueryAsync_ShouldReturnEntities()
    {
        // Arrange
        await _connection.ExecuteAsync(
            "INSERT INTO TestEntities (Id, Name) VALUES (@Id, @Name)",
            new[]
            {
                new { Id = 1, Name = "Entity 1" },
                new { Id = 2, Name = "Entity 2" },
                new { Id = 3, Name = "Entity 3" }
            });

        // Act
        var results = await _sqlExecutor.QueryAsync<TestEntity>(
            "SELECT Id, Name FROM TestEntities ORDER BY Id");

        // Assert
        var resultsList = results.AsList();
        Assert.That(resultsList, Has.Count.EqualTo(3));
        Assert.That(resultsList[0].Id, Is.EqualTo(1));
        Assert.That(resultsList[0].Name, Is.EqualTo("Entity 1"));
        Assert.That(resultsList[1].Id, Is.EqualTo(2));
        Assert.That(resultsList[1].Name, Is.EqualTo("Entity 2"));
        Assert.That(resultsList[2].Id, Is.EqualTo(3));
        Assert.That(resultsList[2].Name, Is.EqualTo("Entity 3"));
    }

    [Test]
    public async Task QueryAsync_ShouldReturnEmptyList_WhenNoEntities()
    {
        // Act
        var results = await _sqlExecutor.QueryAsync<TestEntity>(
            "SELECT Id, Name FROM TestEntities");

        // Assert
        Assert.That(results, Is.Empty);
    }

    [Test]
    public async Task QueryAsync_ShouldPropagateError_WhenQueryFails()
    {
        // Act
        var exception = Assert.ThrowsAsync<SqliteException>(async () =>
            await _sqlExecutor.QueryAsync<TestEntity>(
                "SELECT Id, Name FROM NonExistentTable"));

        // Assert
        Assert.That(exception.Message, Does.Contain("no such table: NonExistentTable"));
    }

    [Test]
    public async Task ExecuteAsync_ShouldReturnRowsAffected()
    {
        // Act
        var result = await _sqlExecutor.ExecuteAsync(
            "INSERT INTO TestEntities (Id, Name) VALUES (@Id, @Name)",
            new { Id = 1, Name = "Test Entity" });

        // Assert
        Assert.That(result, Is.EqualTo(1));

        var entity = await _connection.QueryFirstOrDefaultAsync<TestEntity>(
            "SELECT Id, Name FROM TestEntities WHERE Id = 1");
        Assert.That(entity, Is.Not.Null);
        Assert.That(entity.Name, Is.EqualTo("Test Entity"));
    }

    [Test]
    public async Task ExecuteAsync_ShouldReturnZero_WhenNoRowsAffected()
    {
        // Act
        var result = await _sqlExecutor.ExecuteAsync(
            "DELETE FROM TestEntities WHERE Id = @Id",
            new { Id = 1 });

        // Assert
        Assert.That(result, Is.EqualTo(0));
    }

    [Test]
    public async Task ExecuteAsync_ShouldPropagateError_WhenQueryFails()
    {
        // Act
        var exception = Assert.ThrowsAsync<SqliteException>(async () =>
            await _sqlExecutor.ExecuteAsync(
                "INSERT INTO NonExistentTable (Id, Name) VALUES (@Id, @Name)",
                new { Id = 1, Name = "Test Entity" }));

        // Assert
        Assert.That(exception.Message, Does.Contain("no such table: NonExistentTable"));
    }

    public class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    private class TestConnectionManager : IConnectionManager<SqliteConnection>
    {
        private readonly string _connectionString;

        public TestConnectionManager(string connectionString)
        {
            _connectionString = connectionString;
        }

        public SqliteConnection GetConnection()
        {
            var connection = new SqliteConnection(_connectionString);
            connection.Open();
            return connection;
        }
    }
}