using System.Data;
using ElearningPlatform.Core.Interfaces.Data;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace ElearningPlatform.Data.Test;

[TestFixture]
public class PostgresqlConnectionManagerTest
{
    [SetUp]
    public void Setup()
    {
        _configuration = Substitute.For<IConfiguration>();
    }

    private IConfiguration _configuration;

    private const string ValidConnectionString =
        "Host=localhost;Port=5432;Database=testdb;Username=testuser;Password=testpass";

    [Test]
    public void Constructor_ThrowsException_WhenConnectionStringIsMissing()
    {
        // Arrange
        _configuration.GetConnectionString("Postgres").Returns((string)null!);

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new PostgresqlConnectionManager(_configuration));
        Assert.That(ex.Message,
            Does.Contain("Postgres connection string is missing in configuration (Parameter 'configuration')"));
    }

    [Test]
    public void GetConnection_ReturnsOpenConnection_WhenValidConnectionStringProvided()
    {
        // Arrange
        _configuration.GetConnectionString("DefaultConnection").Returns(ValidConnectionString);
        var mockConnection = Substitute.For<IDbConnection>();
        mockConnection.State.Returns(ConnectionState.Open);

        var manager = new PostgresqlConnectionManager(_configuration);
        var mockManager = Substitute.For<IConnectionManager<IDbConnection>>();
        mockManager.GetConnection().Returns(mockConnection);

        // Act
        using var connection = mockManager.GetConnection();

        // Assert
        Assert.IsNotNull(connection);
        Assert.That(connection, Is.InstanceOf<IDbConnection>());
        Assert.That(connection.State, Is.EqualTo(ConnectionState.Open));
        mockManager.Received().GetConnection();
    }

    [Test]
    public void GetConnection_ThrowsException_WhenConnectionFails()
    {
        // Arrange
        _configuration.GetConnectionString("DefaultConnection").Returns("InvalidConnectionString");
        var manager = new PostgresqlConnectionManager(_configuration);

        // Act & Assert
        var ex = Assert.Throws<Exception>(() => manager.GetConnection());
        Assert.That(ex.Message, Does.Contain("Error connecting to database"));
    }
}