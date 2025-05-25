using System.Security.Claims;
using AscendDev.Core.DTOs.CodeExecution;
using AscendDev.Core.Interfaces.Services;
using AscendDev.Core.Models.CodeExecution;
using AscendDev.Functions.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace AscendDev.API.Test.Controllers;

[TestFixture]
public class TestsControllerTests
{
    private Mock<ICodeTestService> _codeTestServiceMock;
    private Mock<ILogger<TestsController>> _loggerMock;
    private TestsController _controller;

    [SetUp]
    public void Setup()
    {
        _codeTestServiceMock = new Mock<ICodeTestService>();
        _loggerMock = new Mock<ILogger<TestsController>>();
        _controller = new TestsController(_codeTestServiceMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task RunTest_WhenLessonIdIsNull_ReturnsBadRequest()
    {
        // Arrange
        var request = new RunTestsRequest
        {
            LessonId = null,
            Code = "Console.WriteLine(\"Hello, World!\");"
        };

        // Act
        var result = await _controller.RunTest(request);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult.Value, Is.EqualTo("LessonId and Code are required"));
    }

    [Test]
    public async Task RunTest_WhenCodeIsNull_ReturnsBadRequest()
    {
        // Arrange
        var request = new RunTestsRequest
        {
            LessonId = "lesson-1",
            Code = null
        };

        // Act
        var result = await _controller.RunTest(request);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult.Value, Is.EqualTo("LessonId and Code are required"));
    }

    [Test]
    public async Task RunTest_WhenRequestIsValid_ReturnsOkResult()
    {
        // Arrange
        var request = new RunTestsRequest
        {
            LessonId = "lesson-1",
            Code = "Console.WriteLine(\"Hello, World!\");"
        };

        var expectedResult = new TestResult
        {
            Success = true,
            TestResults = new List<TestCaseResult>
            {
                new TestCaseResult
                {
                    TestName = "Test1",
                    Passed = true,
                    Message = "Test passed"
                }
            }
        };

        _codeTestServiceMock.Setup(x => x.RunTestsAsync(request.LessonId, request.Code, null))
            .ReturnsAsync(expectedResult);

        // Setup controller context with unauthenticated user
        var user = new ClaimsPrincipal(new ClaimsIdentity());
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = await _controller.RunTest(request);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.EqualTo(expectedResult));
    }

    [Test]
    public async Task RunTest_WhenUserIsAuthenticated_PassesUserIdToService()
    {
        // Arrange
        var request = new RunTestsRequest
        {
            LessonId = "lesson-1",
            Code = "Console.WriteLine(\"Hello, World!\");"
        };

        var userId = Guid.NewGuid();
        var expectedResult = new TestResult
        {
            Success = true,
            TestResults = new List<TestCaseResult>
            {
                new TestCaseResult
                {
                    TestName = "Test1",
                    Passed = true,
                    Message = "Test passed"
                }
            }
        };

        _codeTestServiceMock.Setup(x => x.RunTestsAsync(request.LessonId, request.Code, userId))
            .ReturnsAsync(expectedResult);

        // Setup controller context with authenticated user
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = await _controller.RunTest(request);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.EqualTo(expectedResult));

        // Verify that the service was called with the correct user ID
        _codeTestServiceMock.Verify(x => x.RunTestsAsync(request.LessonId, request.Code, userId), Times.Once);
    }

    [Test]
    public async Task RunTestAuthenticated_WhenLessonIdIsNull_ReturnsBadRequest()
    {
        // Arrange
        var request = new RunTestsRequest
        {
            LessonId = null,
            Code = "Console.WriteLine(\"Hello, World!\");"
        };

        // Setup controller context with authenticated user
        var userId = Guid.NewGuid();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = await _controller.RunTestAuthenticated(request);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult.Value, Is.EqualTo("LessonId and Code are required"));
    }

    [Test]
    public async Task RunTestAuthenticated_WhenUserIdClaimIsMissing_ReturnsBadRequest()
    {
        // Arrange
        var request = new RunTestsRequest
        {
            LessonId = "lesson-1",
            Code = "Console.WriteLine(\"Hello, World!\");"
        };

        // Setup controller context with authenticated user but missing NameIdentifier claim
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = await _controller.RunTestAuthenticated(request);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult.Value, Is.EqualTo("Invalid user authentication"));
    }

    [Test]
    public async Task RunTestAuthenticated_WhenRequestIsValid_ReturnsOkResult()
    {
        // Arrange
        var request = new RunTestsRequest
        {
            LessonId = "lesson-1",
            Code = "Console.WriteLine(\"Hello, World!\");"
        };

        var userId = Guid.NewGuid();
        var expectedResult = new TestResult
        {
            Success = true,
            TestResults = new List<TestCaseResult>
            {
                new TestCaseResult
                {
                    TestName = "Test1",
                    Passed = true,
                    Message = "Test passed"
                }
            }
        };

        _codeTestServiceMock.Setup(x => x.RunTestsAsync(request.LessonId, request.Code, userId))
            .ReturnsAsync(expectedResult);

        // Setup controller context with authenticated user
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = await _controller.RunTestAuthenticated(request);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.EqualTo(expectedResult));

        // Verify that the service was called with the correct user ID
        _codeTestServiceMock.Verify(x => x.RunTestsAsync(request.LessonId, request.Code, userId), Times.Once);
    }
}