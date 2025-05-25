using System.Text;
using AscendDev.Core.Interfaces.Services;
using AscendDev.Services.Services;
using Microsoft.Extensions.Caching.Distributed;
using Moq;

namespace AscendDev.Services.Test.Services;

[TestFixture]
public class CachingServiceTests
{
    [SetUp]
    public void Setup()
    {
        _cacheMock = new Mock<IDistributedCache>();
        _cachingService = new CachingService(_cacheMock.Object);
    }

    private Mock<IDistributedCache> _cacheMock;
    private ICachingService _cachingService;

    [Test]
    public async Task GetOrCreateAsync_WhenFactoryReturnsNull_DoesNotCache()
    {
        // Arrange
        var key = "null-result-key";

        _cacheMock.Setup(x => x.Get(key))
            .Returns((byte[])null);

        // Act
        var result = await _cachingService.GetOrCreateAsync<TestObject>(
            key,
            () => Task.FromResult<TestObject>(null)
        );

        // Assert
        Assert.That(result, Is.Null);

        // Verify that nothing was cached
        _cacheMock.Verify(x => x.Set(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>()),
            Times.Never);
    }

    [Test]
    public async Task RemoveAsync_CallsCacheRemove()
    {
        // Arrange
        var key = "test-key";

        _cacheMock.Setup(x => x.RemoveAsync(key, default))
            .Returns(Task.CompletedTask);

        // Act
        await _cachingService.RemoveAsync(key);

        // Assert
        _cacheMock.Verify(x => x.RemoveAsync(key, default), Times.Once);
    }

    [Test]
    public async Task ExistsAsync_WhenKeyExists_ReturnsTrue()
    {
        // Arrange
        var key = "existing-key";

        _cacheMock.Setup(x => x.Get(key))
            .Returns(Encoding.UTF8.GetBytes("some-value"));

        // Act
        var result = await _cachingService.ExistsAsync(key);

        // Assert
        Assert.That(result, Is.True);
    }


    // Test class
    private class TestObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}