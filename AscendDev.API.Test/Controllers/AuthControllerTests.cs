using AscendDev.Core.DTOs.Auth;
using AscendDev.Core.Interfaces.Services;
using AscendDev.Functions.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace AscendDev.API.Test.Controllers;

[TestFixture]
public class AuthControllerTests
{
    private Mock<IAuthService> _authServiceMock;
    private AuthController _controller;

    [SetUp]
    public void Setup()
    {
        _authServiceMock = new Mock<IAuthService>();
        _controller = new AuthController(_authServiceMock.Object);
    }

    [Test]
    public async Task Register_WhenRegistrationIsSuccessful_ReturnsCreatedResult()
    {
        // Arrange
        var request = new RegistrationRequest
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        var authResult = new AuthResult
        {
            AccessToken = "jwt-token",
            RefreshToken = "refresh-token",
            User = new UserDto
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                Username = "testuser"
            }
        };

        _authServiceMock.Setup(x => x.RegisterAsync(request))
            .ReturnsAsync(authResult);

        // Act
        var result = await _controller.Register(request);

        // Assert
        Assert.That(result, Is.InstanceOf<CreatedResult>());
        var createdResult = result as CreatedResult;
        Assert.That(createdResult, Is.Not.Null);
        Assert.That(createdResult.Value, Is.EqualTo(authResult));
        Assert.That(createdResult.Location, Is.EqualTo("api/auth/register"));
    }

    [Test]
    public async Task Login_WhenLoginIsSuccessful_ReturnsOkResult()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        var authResult = new AuthResult
        {
            AccessToken = "jwt-token",
            RefreshToken = "refresh-token",
            User = new UserDto
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                Username = "testuser"
            }
        };

        _authServiceMock.Setup(x => x.LoginAsync(request))
            .ReturnsAsync(authResult);

        // Act
        var result = await _controller.Login(request);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.EqualTo(authResult));
    }

    [Test]
    public async Task RefreshToken_WhenRefreshIsSuccessful_ReturnsOkResult()
    {
        // Arrange
        var request = new RefreshTokenRequest
        {
            RefreshToken = "refresh-token"
        };

        var authResult = new AuthResult
        {
            AccessToken = "new-jwt-token",
            RefreshToken = "new-refresh-token",
            User = new UserDto
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                Username = "testuser"
            }
        };

        _authServiceMock.Setup(x => x.RefreshTokenAsync(request.RefreshToken))
            .ReturnsAsync(authResult);

        // Act
        var result = await _controller.RefreshToken(request);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.EqualTo(authResult));
    }

    [Test]
    public async Task RevokeToken_WhenRevokeIsSuccessful_ReturnsOkResult()
    {
        // Arrange
        var request = new RevokeTokenRequest
        {
            RefreshToken = "refresh-token"
        };

        _authServiceMock.Setup(x => x.RevokeRefreshTokenAsync(request.RefreshToken))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.RevokeToken(request);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.EqualTo("Token revoked successfully."));
    }
}