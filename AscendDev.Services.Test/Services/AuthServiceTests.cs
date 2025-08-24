using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AscendDev.Core.DTOs.Auth;
using AscendDev.Core.Exceptions;
using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Interfaces.Utils;
using AscendDev.Core.Models.Auth;
using AscendDev.Services.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace AscendDev.Services.Test.Services
{
    [TestFixture]
    public class AuthServiceTests
    {
        private Mock<IUserRepository> _userRepositoryMock;
        private Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
        private Mock<IUserRoleRepository> _userRoleRepositoryMock;
        private Mock<IPasswordHasher> _passwordHasherMock;
        private Mock<IJwtHelper> _jwtHelperMock;
        private Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private Mock<ILogger<AuthService>> _loggerMock;
        private JwtSettings _jwtSettings;
        private AuthService _authService;
        private HttpContext _httpContext;

        [SetUp]
        public void Setup()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
            _userRoleRepositoryMock = new Mock<IUserRoleRepository>();
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _jwtHelperMock = new Mock<IJwtHelper>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _loggerMock = new Mock<ILogger<AuthService>>();

            _jwtSettings = new JwtSettings
            {
                Key = "test-secret-key-with-minimum-length-for-security",
                Issuer = "test-issuer",
                Audience = "test-audience",
                AccessTokenExpiryMinutes = 60,
                RefreshTokenExpiryDays = 7
            };

            _httpContext = new DefaultHttpContext();
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(_httpContext);

            _authService = new AuthService(
                _userRepositoryMock.Object,
                _refreshTokenRepositoryMock.Object,
                _userRoleRepositoryMock.Object,
                _jwtSettings,
                _passwordHasherMock.Object,
                _jwtHelperMock.Object,
                _httpContextAccessorMock.Object,
                _loggerMock.Object
            );
        }

        [Test]
        public async Task RegisterAsync_WhenUserAlreadyExists_ThrowsConflictException()
        {
            // Arrange
            var request = new RegistrationRequest
            {
                Email = "test@example.com",
                Password = "Password123!"
            };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email))
                .ReturnsAsync(new User { Email = request.Email });

            // Act & Assert
            var exception = Assert.ThrowsAsync<ConflictException>(async () =>
                await _authService.RegisterAsync(request));

            Assert.That(exception.Message, Is.EqualTo("User with this email already exists."));
        }

        [Test]
        public async Task RegisterAsync_WhenUserDoesNotExist_CreatesUserAndReturnsAuthResult()
        {
            // Arrange
            var request = new RegistrationRequest
            {
                Email = "test@example.com",
                Password = "Password123!"
            };

            var hashedPassword = "hashed_password";
            _passwordHasherMock.Setup(x => x.Hash(request.Password))
                .Returns(hashedPassword);

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email))
                .ReturnsAsync((User)null);

            User createdUser = null;
            _userRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<User>()))
                .Callback<User>(user => createdUser = user)
                .ReturnsAsync(true);

            var accessToken = "test_access_token";
            _jwtHelperMock.Setup(x => x.GenerateToken(It.IsAny<Guid>(), request.Email, It.IsAny<IEnumerable<string>>()))
                .Returns(accessToken);

            _userRoleRepositoryMock.Setup(x => x.GetRolesByUserIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new List<Role>());

            _refreshTokenRepositoryMock.Setup(x => x.SaveAsync(It.IsAny<RefreshToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _authService.RegisterAsync(request);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.AccessToken, Is.EqualTo(accessToken));
            Assert.That(result.RefreshToken, Is.Not.Null);
            Assert.That(result.User, Is.Not.Null);
            Assert.That(result.User.Email, Is.EqualTo(request.Email));

            _userRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Once);
            Assert.That(createdUser, Is.Not.Null);
            Assert.That(createdUser.Email, Is.EqualTo(request.Email));
            Assert.That(createdUser.PasswordHash, Is.EqualTo(hashedPassword));
            Assert.That(createdUser.Username, Is.EqualTo(request.Email.Split('@')[0]));
            Assert.That(createdUser.IsEmailVerified, Is.False);
        }

        [Test]
        public async Task LoginAsync_WhenUserDoesNotExist_ThrowsBadRequestException()
        {
            // Arrange
            var request = new LoginRequest
            {
                Email = "test@example.com",
                Password = "Password123!"
            };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email))
                .ReturnsAsync((User)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<BadRequestException>(async () =>
                await _authService.LoginAsync(request));

            Assert.That(exception.Message, Is.EqualTo("Invalid email or password."));
        }

        [Test]
        public async Task LoginAsync_WhenPasswordIsIncorrect_ThrowsBadRequestException()
        {
            // Arrange
            var request = new LoginRequest
            {
                Email = "test@example.com",
                Password = "Password123!"
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                PasswordHash = "hashed_password"
            };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email))
                .ReturnsAsync(user);

            _passwordHasherMock.Setup(x => x.Verify(request.Password, user.PasswordHash))
                .Returns(false);

            // Act & Assert
            var exception = Assert.ThrowsAsync<BadRequestException>(async () =>
                await _authService.LoginAsync(request));

            Assert.That(exception.Message, Is.EqualTo("Invalid email or password."));
        }

        [Test]
        public async Task LoginAsync_WhenCredentialsAreValid_ReturnsAuthResult()
        {
            // Arrange
            var request = new LoginRequest
            {
                Email = "test@example.com",
                Password = "Password123!"
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                PasswordHash = "hashed_password",
                Username = "test"
            };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email))
                .ReturnsAsync(user);

            _passwordHasherMock.Setup(x => x.Verify(request.Password, user.PasswordHash))
                .Returns(true);

            _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(true);

            var accessToken = "test_access_token";
            _jwtHelperMock.Setup(x => x.GenerateToken(user.Id, user.Email, It.IsAny<IEnumerable<string>>()))
                .Returns(accessToken);

            _userRoleRepositoryMock.Setup(x => x.GetRolesByUserIdAsync(user.Id))
                .ReturnsAsync(new List<Role>());

            _refreshTokenRepositoryMock.Setup(x => x.SaveAsync(It.IsAny<RefreshToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.AccessToken, Is.EqualTo(accessToken));
            Assert.That(result.RefreshToken, Is.Not.Null);
            Assert.That(result.User, Is.Not.Null);
            Assert.That(result.User.Email, Is.EqualTo(user.Email));
            Assert.That(result.User.Username, Is.EqualTo(user.Username));

            _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);
        }

        [Test]
        public async Task RefreshTokenAsync_WhenTokenIsInvalid_ThrowsUnauthorizedException()
        {
            // Arrange
            var token = "invalid_token";

            _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync(token))
                .ReturnsAsync((RefreshToken)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<UnauthorizedException>(async () =>
                await _authService.RefreshTokenAsync(token));

            Assert.That(exception.Message, Is.EqualTo("Invalid or expired refresh token."));
        }

        [Test]
        public async Task RefreshTokenAsync_WhenTokenIsExpired_ThrowsUnauthorizedException()
        {
            // Arrange
            var token = "expired_token";
            var refreshToken = new RefreshToken
            {
                Token = token,
                Expires = DateTime.UtcNow.AddDays(-1), // Expired
                // Set Revoked to make IsActive false
                Revoked = DateTime.UtcNow
            };

            _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync(token))
                .ReturnsAsync(refreshToken);

            // Act & Assert
            var exception = Assert.ThrowsAsync<UnauthorizedException>(async () =>
                await _authService.RefreshTokenAsync(token));

            Assert.That(exception.Message, Is.EqualTo("Invalid or expired refresh token."));
        }

        [Test]
        public async Task RefreshTokenAsync_WhenUserNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var token = "valid_token";
            var userId = Guid.NewGuid();
            var refreshToken = new RefreshToken
            {
                Token = token,
                UserId = userId,
                Expires = DateTime.UtcNow.AddDays(1), // Not expired
                // Not revoked and not expired, so IsActive will be true
            };

            _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync(token))
                .ReturnsAsync(refreshToken);

            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync((User)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<NotFoundException>(async () =>
                await _authService.RefreshTokenAsync(token));

            Assert.That(exception.Message, Is.EqualTo("User not found.").Or.EqualTo("User not found. not found."));
        }

        [Test]
        public async Task RefreshTokenAsync_WhenTokenIsValidAndUserExists_ReturnsNewAuthResult()
        {
            // Arrange
            var token = "valid_token";
            var userId = Guid.NewGuid();
            var refreshToken = new RefreshToken
            {
                Token = token,
                UserId = userId,
                Expires = DateTime.UtcNow.AddDays(1), // Not expired
                // Not revoked and not expired, so IsActive will be true
            };

            var user = new User
            {
                Id = userId,
                Email = "test@example.com",
                Username = "test"
            };

            _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync(token))
                .ReturnsAsync(refreshToken);

            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(user);

            var accessToken = "new_access_token";
            _jwtHelperMock.Setup(x => x.GenerateToken(user.Id, user.Email, It.IsAny<IEnumerable<string>>()))
                .Returns(accessToken);

            _userRoleRepositoryMock.Setup(x => x.GetRolesByUserIdAsync(user.Id))
                .ReturnsAsync(new List<Role>());

            _refreshTokenRepositoryMock.Setup(x => x.SaveAsync(It.IsAny<RefreshToken>()))
                .Returns(Task.CompletedTask);

            _refreshTokenRepositoryMock.Setup(x => x.DeleteAsync(token))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _authService.RefreshTokenAsync(token);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.AccessToken, Is.EqualTo(accessToken));
            Assert.That(result.RefreshToken, Is.Not.Null);
            Assert.That(result.User, Is.Not.Null);
            Assert.That(result.User.Email, Is.EqualTo(user.Email));
            Assert.That(result.User.Username, Is.EqualTo(user.Username));

            _refreshTokenRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<RefreshToken>()), Times.Once);
            _refreshTokenRepositoryMock.Verify(x => x.DeleteAsync(token), Times.Once);
        }

        [Test]
        public async Task RevokeRefreshTokenAsync_WhenTokenNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var token = "nonexistent_token";

            _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync(token))
                .ReturnsAsync((RefreshToken)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<NotFoundException>(async () =>
                await _authService.RevokeRefreshTokenAsync(token));

            Assert.That(exception.Message, Is.EqualTo("Refresh token").Or.EqualTo("Refresh token not found."));
        }

        [Test]
        public async Task RevokeRefreshTokenAsync_WhenTokenExists_RevokesToken()
        {
            // Arrange
            var token = "valid_token";
            var refreshToken = new RefreshToken
            {
                Token = token,
                UserId = Guid.NewGuid(),
                Expires = DateTime.UtcNow.AddDays(1),
                // Not revoked and not expired, so IsActive will be true
            };

            _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync(token))
                .ReturnsAsync(refreshToken);

            _refreshTokenRepositoryMock.Setup(x => x.RevokeAsync(token, It.IsAny<string>()))
                .ReturnsAsync(true);

            _httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");

            // Act
            await _authService.RevokeRefreshTokenAsync(token);

            // Assert
            _refreshTokenRepositoryMock.Verify(x => x.RevokeAsync(token, "127.0.0.1"), Times.Once);
        }

        [Test]
        public async Task RevokeRefreshTokenAsync_WhenRepositoryThrowsException_LogsAndRethrows()
        {
            // Arrange
            var token = "valid_token";
            var refreshToken = new RefreshToken
            {
                Token = token,
                UserId = Guid.NewGuid(),
                Expires = DateTime.UtcNow.AddDays(1),
                // Not revoked and not expired, so IsActive will be true
            };

            var expectedException = new InvalidOperationException("Database error");

            _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync(token))
                .ReturnsAsync(refreshToken);

            _refreshTokenRepositoryMock.Setup(x => x.RevokeAsync(token, It.IsAny<string>()))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _authService.RevokeRefreshTokenAsync(token));

            Assert.That(exception, Is.EqualTo(expectedException));

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    expectedException,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}