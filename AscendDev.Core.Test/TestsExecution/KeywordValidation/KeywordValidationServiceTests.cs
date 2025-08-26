using AscendDev.Core.Models.TestsExecution.KeywordValidation;
using AscendDev.Core.TestsExecution.KeywordValidation;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace AscendDev.Core.Test.TestsExecution.KeywordValidation;

public class KeywordValidationServiceTests
{
    private readonly KeywordValidationService _service;
    private readonly Mock<ILogger<KeywordValidationService>> _mockLogger;

    public KeywordValidationServiceTests()
    {
        _mockLogger = new Mock<ILogger<KeywordValidationService>>();
        _service = new KeywordValidationService(_mockLogger.Object);
    }

    [Test]
    public async Task ValidateKeywordsAsync_TypeScriptWithSpecialCharacters_ShouldValidateCorrectly()
    {
        // Arrange
        var code = @"function reverseArray<T>(arr: T[]): T[] {
    return arr.slice().reverse();
}

function findMax(arr: number[]): number | undefined {
    if (arr.length === 0) return undefined;
    return Math.max(...arr);
}

function sumArray(arr: number[]): number {
    return arr.reduce((sum, num) => sum + num, 0);
}";

        var requirements = new List<KeywordRequirement>
        {
            new KeywordRequirement
            {
                Keyword = "function",
                Description = "Must use function declarations",
                Required = true,
                CaseSensitive = true,
                AllowPartialMatch = false,
                MinOccurrences = 3
            },
            new KeywordRequirement
            {
                Keyword = ":",
                Description = "Must use type annotations",
                Required = true,
                CaseSensitive = true,
                AllowPartialMatch = false,
                MinOccurrences = 5
            }
        };

        // Act
        var result = await _service.ValidateKeywordsAsync(code, "typescript", requirements);

        // Assert
        Assert.That(result.IsValid, Is.True);
        Assert.That(result.Errors, Is.Empty);

        // Verify function keyword matches
        var functionMatches = result.Matches.Where(m => m.Keyword == "function").ToList();
        Assert.That(functionMatches.Count, Is.EqualTo(3));

        // Verify colon keyword matches (should find at least 5)
        var colonMatches = result.Matches.Where(m => m.Keyword == ":").ToList();
        Assert.That(colonMatches.Count, Is.GreaterThanOrEqualTo(5), $"Expected at least 5 colon matches, but found {colonMatches.Count}");
    }

    [Test]
    public async Task ValidateKeywordsAsync_TypeScriptWithInsufficientColons_ShouldFail()
    {
        // Arrange
        var code = @"function reverseArray(arr) {
    return arr.slice().reverse();
}";

        var requirements = new List<KeywordRequirement>
        {
            new KeywordRequirement
            {
                Keyword = ":",
                Description = "Must use type annotations",
                Required = true,
                CaseSensitive = true,
                AllowPartialMatch = false,
                MinOccurrences = 5
            }
        };

        // Act
        var result = await _service.ValidateKeywordsAsync(code, "typescript", requirements);

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Count, Is.EqualTo(1));
        Assert.That(result.Errors[0].ErrorType, Is.EqualTo(KeywordErrorType.Missing));
        Assert.That(result.Errors[0].Keyword, Does.Contain(":"));
    }

    [Test]
    public async Task ValidateKeywordsAsync_CSharpWithSpecialCharacters_ShouldValidateCorrectly()
    {
        // Arrange
        var code = @"public class TestClass
{
    public int[] ReverseArray(int[] array)
    {
        return array.Reverse().ToArray();
    }
}";

        var requirements = new List<KeywordRequirement>
        {
            new KeywordRequirement
            {
                Keyword = "public",
                Description = "Must use public access modifiers",
                Required = true,
                CaseSensitive = true,
                AllowPartialMatch = false,
                MinOccurrences = 2
            },
            new KeywordRequirement
            {
                Keyword = "int[]",
                Description = "Must work with integer arrays",
                Required = true,
                CaseSensitive = true,
                AllowPartialMatch = false,
                MinOccurrences = 2
            }
        };

        // Act
        var result = await _service.ValidateKeywordsAsync(code, "csharp", requirements);

        // Assert
        Assert.That(result.IsValid, Is.True);
        Assert.That(result.Errors, Is.Empty);

        // Verify public keyword matches
        var publicMatches = result.Matches.Where(m => m.Keyword == "public").ToList();
        Assert.That(publicMatches.Count, Is.EqualTo(2));

        // Verify int[] keyword matches
        var intArrayMatches = result.Matches.Where(m => m.Keyword == "int[]").ToList();
        Assert.That(intArrayMatches.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task ValidateKeywordsAsync_PythonWithWordBoundaries_ShouldValidateCorrectly()
    {
        // Arrange
        var code = @"def reverse_array(arr):
    return arr[::-1]

def find_max(arr):
    if not arr:
        return None
    return max(arr)

def sum_array(arr):
    return sum(arr)";

        var requirements = new List<KeywordRequirement>
        {
            new KeywordRequirement
            {
                Keyword = "def",
                Description = "Must use function definitions",
                Required = true,
                CaseSensitive = true,
                AllowPartialMatch = false,
                MinOccurrences = 3
            }
        };

        // Act
        var result = await _service.ValidateKeywordsAsync(code, "python", requirements);

        // Assert
        Assert.That(result.IsValid, Is.True);
        Assert.That(result.Errors, Is.Empty);

        // Verify def keyword matches
        var defMatches = result.Matches.Where(m => m.Keyword == "def").ToList();
        Assert.That(defMatches.Count, Is.EqualTo(3));
    }

    [Test]
    public async Task ValidateKeywordsAsync_UnsupportedLanguage_ShouldThrowException()
    {
        // Arrange
        var code = "some code";
        var requirements = new List<KeywordRequirement>();

        // Act & Assert
        Assert.ThrowsAsync<NotSupportedException>(
            () => _service.ValidateKeywordsAsync(code, "unsupported", requirements));
    }

    [Test]
    public async Task ValidateKeywordsAsync_EmptyRequirements_ShouldReturnValid()
    {
        // Arrange
        var code = "some code";
        var requirements = new List<KeywordRequirement>();

        // Act
        var result = await _service.ValidateKeywordsAsync(code, "typescript", requirements);

        // Assert
        Assert.That(result.IsValid, Is.True);
        Assert.That(result.Errors, Is.Empty);
        Assert.That(result.Matches, Is.Empty);
    }

    [Test]
    public async Task ValidateKeywordsAsync_NonRequiredKeywords_ShouldBeIgnored()
    {
        // Arrange
        var code = "function test() {}";
        var requirements = new List<KeywordRequirement>
        {
            new KeywordRequirement
            {
                Keyword = "function",
                Description = "Optional function keyword",
                Required = false,
                CaseSensitive = true,
                AllowPartialMatch = false,
                MinOccurrences = 1
            }
        };

        // Act
        var result = await _service.ValidateKeywordsAsync(code, "typescript", requirements);

        // Assert
        Assert.That(result.IsValid, Is.True);
        Assert.That(result.Errors, Is.Empty);
        Assert.That(result.Matches, Is.Empty); // Non-required keywords are not processed
    }
}