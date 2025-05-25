using AscendDev.Functions.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace AscendDev.API.Test.Controllers;

[TestFixture]
public class HealthControllerTests
{
    private HealthController _controller;

    [SetUp]
    public void Setup()
    {
        _controller = new HealthController();
    }

    [Test]
    public void GetHealth_ReturnsOkResult()
    {
        // Act
        var result = _controller.GetHealth();

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public void GetHealth_ReturnsHealthyMessage()
    {
        // Act
        var result = _controller.GetHealth() as OkObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Value, Is.EqualTo("Healthy"));
    }
}