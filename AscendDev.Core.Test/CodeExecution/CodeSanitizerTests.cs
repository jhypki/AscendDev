using System.Security;
using AscendDev.Core.CodeExecution;
using AscendDev.Core.Constants;
using NUnit.Framework;

namespace AscendDev.Core.Test.CodeExecution;

[TestFixture]
public class CodeSanitizerTests
{
    [Test]
    public void SanitizeCode_WhenCodeIsNull_ThrowsArgumentException()
    {
        // Arrange
        string code = null;
        string language = SupportedLanguages.CSharp;

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => CodeSanitizer.SanitizeCode(code, language));
        Assert.That(ex.ParamName, Is.EqualTo("code"));
        Assert.That(ex.Message, Does.Contain("Code cannot be null or empty"));
    }

    [Test]
    public void SanitizeCode_WhenCodeIsEmpty_ThrowsArgumentException()
    {
        // Arrange
        string code = string.Empty;
        string language = SupportedLanguages.CSharp;

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => CodeSanitizer.SanitizeCode(code, language));
        Assert.That(ex.ParamName, Is.EqualTo("code"));
        Assert.That(ex.Message, Does.Contain("Code cannot be null or empty"));
    }

    [Test]
    public void SanitizeCode_WhenLanguageIsNull_ThrowsArgumentException()
    {
        // Arrange
        string code = "Console.WriteLine(\"Hello, World!\");";
        string language = null;

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => CodeSanitizer.SanitizeCode(code, language));
        Assert.That(ex.ParamName, Is.EqualTo("language"));
        Assert.That(ex.Message, Does.Contain("Language cannot be null or empty"));
    }

    [Test]
    public void SanitizeCode_WhenLanguageIsEmpty_ThrowsArgumentException()
    {
        // Arrange
        string code = "Console.WriteLine(\"Hello, World!\");";
        string language = string.Empty;

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => CodeSanitizer.SanitizeCode(code, language));
        Assert.That(ex.ParamName, Is.EqualTo("language"));
        Assert.That(ex.Message, Does.Contain("Language cannot be null or empty"));
    }

    [Test]
    public void SanitizeCode_WhenLanguageIsNotSupported_ThrowsNotSupportedException()
    {
        // Arrange
        string code = "print('Hello, World!')";
        string language = "ruby";

        // Act & Assert
        var ex = Assert.Throws<NotSupportedException>(() => CodeSanitizer.SanitizeCode(code, language));
        Assert.That(ex.Message, Does.Contain("Sanitization for language 'ruby' is not supported"));
    }

    [Test]
    public void SanitizeCode_WhenCodeContainsCommonForbiddenPattern_ThrowsSecurityException()
    {
        // Arrange
        string code = "var result = eval('2 + 2');";
        string language = SupportedLanguages.JavaScript;

        // Act & Assert
        var ex = Assert.Throws<SecurityException>(() => CodeSanitizer.SanitizeCode(code, language));
        Assert.That(ex.Message, Does.Contain("Code contains potentially unsafe operation"));
    }

    [Test]
    public void SanitizeCode_WhenPythonCodeContainsForbiddenPattern_ThrowsSecurityException()
    {
        // Arrange
        string code = "import os\nos.system('rm -rf /')";
        string language = SupportedLanguages.Python;

        // Act & Assert
        var ex = Assert.Throws<SecurityException>(() => CodeSanitizer.SanitizeCode(code, language));
        Assert.That(ex.Message, Does.Contain("Python code contains potentially unsafe operation"));
    }

    [Test]
    public void SanitizeCode_WhenCSharpCodeContainsForbiddenPattern_ThrowsSecurityException()
    {
        // Arrange
        string code = "using System.Diagnostics;\nSystem.Diagnostics.Process.Start(\"cmd.exe\");";
        string language = SupportedLanguages.CSharp;

        // Act & Assert
        var ex = Assert.Throws<SecurityException>(() => CodeSanitizer.SanitizeCode(code, language));
        Assert.That(ex.Message, Does.Contain("C# code contains potentially unsafe operation"));
    }

    [Test]
    public void SanitizeCode_WhenJavaScriptCodeContainsForbiddenPattern_ThrowsSecurityException()
    {
        // Arrange
        string code = "const fs = require('fs');\nfs.readFileSync('/etc/passwd');";
        string language = SupportedLanguages.JavaScript;

        // Act & Assert
        var ex = Assert.Throws<SecurityException>(() => CodeSanitizer.SanitizeCode(code, language));
        Assert.That(ex.Message, Does.Contain("JavaScript/TypeScript code contains potentially unsafe operation"));
    }

    [Test]
    public void SanitizeCode_WhenTypeScriptCodeContainsForbiddenPattern_ThrowsSecurityException()
    {
        // Arrange
        string code = "import * as fs from 'fs';\nfs.readFileSync('/etc/passwd');";
        string language = SupportedLanguages.TypeScript;

        // Act & Assert
        var ex = Assert.Throws<SecurityException>(() => CodeSanitizer.SanitizeCode(code, language));
        Assert.That(ex.Message, Does.Contain("JavaScript/TypeScript code contains potentially unsafe operation"));
    }

    [Test]
    public void SanitizeCode_WhenPythonCodeIsSafe_ReturnsSameCode()
    {
        // Arrange
        string code = "def add(a, b):\n    return a + b\n\nresult = add(2, 3)\nprint(result)";
        string language = SupportedLanguages.Python;

        // Act
        string result = CodeSanitizer.SanitizeCode(code, language);

        // Assert
        Assert.That(result, Is.EqualTo(code));
    }

    [Test]
    public void SanitizeCode_WhenCSharpCodeIsSafe_ReturnsSameCode()
    {
        // Arrange
        string code = "public class Calculator\n{\n    public int Add(int a, int b)\n    {\n        return a + b;\n    }\n}";
        string language = SupportedLanguages.CSharp;

        // Act
        string result = CodeSanitizer.SanitizeCode(code, language);

        // Assert
        Assert.That(result, Is.EqualTo(code));
    }

    [Test]
    public void SanitizeCode_WhenJavaScriptCodeIsSafe_ReturnsSameCode()
    {
        // Arrange
        string code = "function add(a, b) {\n    return a + b;\n}\n\nconst result = add(2, 3);\nconsole.log(result);";
        string language = SupportedLanguages.JavaScript;

        // Act
        string result = CodeSanitizer.SanitizeCode(code, language);

        // Assert
        Assert.That(result, Is.EqualTo(code));
    }

    [Test]
    public void SanitizeCode_WhenTypeScriptCodeIsSafe_ReturnsSameCode()
    {
        // Arrange
        string code = "function add(a: number, b: number): number {\n    return a + b;\n}\n\nconst result = add(2, 3);\nconsole.log(result);";
        string language = SupportedLanguages.TypeScript;

        // Act
        string result = CodeSanitizer.SanitizeCode(code, language);

        // Assert
        Assert.That(result, Is.EqualTo(code));
    }
}