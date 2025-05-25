using AscendDev.Core.DTOs.CodeExecution;
using AscendDev.Core.Interfaces.Services;
using AscendDev.Core.Models.CodeExecution;
using AscendDev.Functions.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace AscendDev.API.Test.Controllers;

[TestFixture]
public class CodeExecutionControllerTests
{
    private Mock<ICodeExecutionService> _codeExecutionServiceMock;
    private CodeExecutionController _controller;

    [SetUp]
    public void Setup()
    {
        _codeExecutionServiceMock = new Mock<ICodeExecutionService>();
        _controller = new CodeExecutionController(_codeExecutionServiceMock.Object);
    }

    [Test]
    public async Task RunCode_WhenLanguageIsNull_ReturnsBadRequest()
    {
        // Arrange
        var request = new RunCodeRequest
        {
            Language = null,
            Code = "Console.WriteLine(\"Hello, World!\");"
        };

        // Act
        var result = await _controller.RunCode(request);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult.Value, Is.EqualTo("Language and Code are required"));
    }

    [Test]
    public async Task RunCode_WhenCodeIsNull_ReturnsBadRequest()
    {
        // Arrange
        var request = new RunCodeRequest
        {
            Language = "csharp",
            Code = null
        };

        // Act
        var result = await _controller.RunCode(request);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult.Value, Is.EqualTo("Language and Code are required"));
    }

    [Test]
    public async Task RunCode_WhenRequestIsValid_ReturnsOkResult()
    {
        // Arrange
        var request = new RunCodeRequest
        {
            Language = "csharp",
            Code = "Console.WriteLine(\"Hello, World!\");"
        };

        var expectedResult = new CodeExecutionResult
        {
            Success = true,
            Stdout = "Hello, World!",
            Stderr = string.Empty,
            ExitCode = 0,
            ExecutionTimeMs = 100
        };

        _codeExecutionServiceMock.Setup(x => x.ExecuteCodeAsync(request.Language, request.Code))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.RunCode(request);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.EqualTo(expectedResult));
    }
}