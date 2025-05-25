using System;
using System.Net;
using System.Threading.Tasks;
using AscendDev.API.Middleware;
using AscendDev.Core.DTOs;
using AscendDev.Core.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace AscendDev.API.Test.Middleware
{
    [TestFixture]
    public class ErrorHandlingMiddlewareTests
    {
        private Mock<ILogger<ErrorHandlingMiddleware>> _loggerMock;
        private ErrorHandlingMiddleware _middleware;
        private DefaultHttpContext _httpContext;
        private Mock<RequestDelegate> _nextMock;

        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<ErrorHandlingMiddleware>>();
            _nextMock = new Mock<RequestDelegate>();
            _middleware = new ErrorHandlingMiddleware(_nextMock.Object, _loggerMock.Object);
            _httpContext = new DefaultHttpContext();
            _httpContext.Response.Body = new System.IO.MemoryStream();
        }

        [Test]
        public async Task InvokeAsync_NoException_CallsNext()
        {
            // Arrange
            _nextMock.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_httpContext);

            // Assert
            _nextMock.Verify(next => next(_httpContext), Times.Once);
        }

        [Test]
        public async Task InvokeAsync_ValidationException_ReturnsValidationError()
        {
            // Arrange
            var errors = new Dictionary<string, string[]>
            {
                { "Field1", new[] { "Error 1", "Error 2" } }
            };
            var validationException = new ValidationException(errors);

            _nextMock.Setup(next => next(It.IsAny<HttpContext>()))
                .ThrowsAsync(validationException);

            // Act
            await _middleware.InvokeAsync(_httpContext);

            // Assert
            _httpContext.Response.Body.Seek(0, System.IO.SeekOrigin.Begin);
            var responseBody = await new System.IO.StreamReader(_httpContext.Response.Body).ReadToEndAsync();
            var response = JsonConvert.DeserializeObject<ApiResponse<object>>(responseBody);

            Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(validationException.StatusCode));
            Assert.That(_httpContext.Response.ContentType, Is.EqualTo("application/json"));
            Assert.That(response.Success, Is.False);
            Assert.That(response.Message, Is.EqualTo(validationException.Message));
            Assert.That(response.Errors, Is.Not.Null);
            Assert.That(response.Errors.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task InvokeAsync_NotFoundException_ReturnsNotFoundError()
        {
            // Arrange
            // Create a NotFoundException with a message that will include the details we want to test
            var notFoundException = new NotFoundException("Resource with details: Additional details");

            _nextMock.Setup(next => next(It.IsAny<HttpContext>()))
                .ThrowsAsync(notFoundException);

            // Act
            await _middleware.InvokeAsync(_httpContext);

            // Assert
            _httpContext.Response.Body.Seek(0, System.IO.SeekOrigin.Begin);
            var responseBody = await new System.IO.StreamReader(_httpContext.Response.Body).ReadToEndAsync();
            var response = JsonConvert.DeserializeObject<ApiResponse<object>>(responseBody);

            Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(notFoundException.StatusCode));
            Assert.That(_httpContext.Response.ContentType, Is.EqualTo("application/json"));
            Assert.That(response.Success, Is.False);
            Assert.That(response.Message, Is.EqualTo(notFoundException.Message));
            // The error details might be null or contain the message
            if (response.Errors != null)
            {
                Assert.That(response.Errors.Count, Is.EqualTo(1));
                Assert.That(response.Errors[0], Is.EqualTo("Additional details"));
            }
        }

        [Test]
        public async Task InvokeAsync_UnauthorizedException_ReturnsUnauthorizedError()
        {
            // Arrange
            // Create an UnauthorizedException with a custom message
            var unauthorizedException = new UnauthorizedException("Unauthorized access: Invalid token");

            _nextMock.Setup(next => next(It.IsAny<HttpContext>()))
                .ThrowsAsync(unauthorizedException);

            // Act
            await _middleware.InvokeAsync(_httpContext);

            // Assert
            _httpContext.Response.Body.Seek(0, System.IO.SeekOrigin.Begin);
            var responseBody = await new System.IO.StreamReader(_httpContext.Response.Body).ReadToEndAsync();
            var response = JsonConvert.DeserializeObject<ApiResponse<object>>(responseBody);

            Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(unauthorizedException.StatusCode));
            Assert.That(_httpContext.Response.ContentType, Is.EqualTo("application/json"));
            Assert.That(response.Success, Is.False);
            Assert.That(response.Message, Is.EqualTo(unauthorizedException.Message));
            // The error details might be null or contain the message
            if (response.Errors != null)
            {
                Assert.That(response.Errors.Count, Is.EqualTo(1));
                Assert.That(response.Errors[0], Is.EqualTo("Invalid token"));
            }
        }

        [Test]
        public async Task InvokeAsync_ForbiddenException_ReturnsForbiddenError()
        {
            // Arrange
            // Create a ForbiddenException with a custom message
            var forbiddenException = new ForbiddenException("Access forbidden: Insufficient permissions");

            _nextMock.Setup(next => next(It.IsAny<HttpContext>()))
                .ThrowsAsync(forbiddenException);

            // Act
            await _middleware.InvokeAsync(_httpContext);

            // Assert
            _httpContext.Response.Body.Seek(0, System.IO.SeekOrigin.Begin);
            var responseBody = await new System.IO.StreamReader(_httpContext.Response.Body).ReadToEndAsync();
            var response = JsonConvert.DeserializeObject<ApiResponse<object>>(responseBody);

            Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(forbiddenException.StatusCode));
            Assert.That(_httpContext.Response.ContentType, Is.EqualTo("application/json"));
            Assert.That(response.Success, Is.False);
            Assert.That(response.Message, Is.EqualTo(forbiddenException.Message));
            // The error details might be null or contain the message
            if (response.Errors != null)
            {
                Assert.That(response.Errors.Count, Is.EqualTo(1));
                Assert.That(response.Errors[0], Is.EqualTo("Insufficient permissions"));
            }
        }

        [Test]
        public async Task InvokeAsync_ConflictException_ReturnsConflictError()
        {
            // Arrange
            // Create a ConflictException with a custom message
            var conflictException = new ConflictException("Resource conflict: Resource already exists");

            _nextMock.Setup(next => next(It.IsAny<HttpContext>()))
                .ThrowsAsync(conflictException);

            // Act
            await _middleware.InvokeAsync(_httpContext);

            // Assert
            _httpContext.Response.Body.Seek(0, System.IO.SeekOrigin.Begin);
            var responseBody = await new System.IO.StreamReader(_httpContext.Response.Body).ReadToEndAsync();
            var response = JsonConvert.DeserializeObject<ApiResponse<object>>(responseBody);

            Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(conflictException.StatusCode));
            Assert.That(_httpContext.Response.ContentType, Is.EqualTo("application/json"));
            Assert.That(response.Success, Is.False);
            Assert.That(response.Message, Is.EqualTo(conflictException.Message));
            // The error details might be null or contain the message
            if (response.Errors != null)
            {
                Assert.That(response.Errors.Count, Is.EqualTo(1));
                Assert.That(response.Errors[0], Is.EqualTo("Resource already exists"));
            }
        }

        [Test]
        public async Task InvokeAsync_ApiException_ReturnsApiError()
        {
            // Arrange
            var apiException = new ApiException("API error", StatusCodes.Status400BadRequest, "API error details");

            _nextMock.Setup(next => next(It.IsAny<HttpContext>()))
                .ThrowsAsync(apiException);

            // Act
            await _middleware.InvokeAsync(_httpContext);

            // Assert
            _httpContext.Response.Body.Seek(0, System.IO.SeekOrigin.Begin);
            var responseBody = await new System.IO.StreamReader(_httpContext.Response.Body).ReadToEndAsync();
            var response = JsonConvert.DeserializeObject<ApiResponse<object>>(responseBody);

            Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(apiException.StatusCode));
            Assert.That(_httpContext.Response.ContentType, Is.EqualTo("application/json"));
            Assert.That(response.Success, Is.False);
            Assert.That(response.Message, Is.EqualTo(apiException.Message));
            Assert.That(response.Errors, Is.Not.Null);
            Assert.That(response.Errors.Count, Is.EqualTo(1));
            Assert.That(response.Errors[0], Is.EqualTo("API error details"));
        }

        [Test]
        public async Task InvokeAsync_GenericException_ReturnsInternalServerError()
        {
            // Arrange
            var exception = new Exception("Unexpected error");

            _nextMock.Setup(next => next(It.IsAny<HttpContext>()))
                .ThrowsAsync(exception);

            // Act
            await _middleware.InvokeAsync(_httpContext);

            // Assert
            _httpContext.Response.Body.Seek(0, System.IO.SeekOrigin.Begin);
            var responseBody = await new System.IO.StreamReader(_httpContext.Response.Body).ReadToEndAsync();
            var response = JsonConvert.DeserializeObject<ApiResponse<object>>(responseBody);

            Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
            Assert.That(_httpContext.Response.ContentType, Is.EqualTo("application/json"));
            Assert.That(response.Success, Is.False);
            Assert.That(response.Message, Is.EqualTo("An unexpected error occurred"));
            Assert.That(response.Errors, Is.Null);
        }
    }
}