# C# Testing Environment

This directory contains the necessary files for the C# testing environment used in AscendDev.

## Components

- `run-tests.sh`: Shell script that runs the tests and converts the results to JSON
- `TrxToJson.cs` and `TrxToJson.csproj`: A simple utility to convert TRX test results to JSON format

## How it works

1. The user's code is placed in the `/app/test` directory as `UserSolution.cs`
2. A test project is created if it doesn't exist
3. Tests are run using `dotnet test` with the TRX logger
4. The TRX results are converted to JSON format for easier parsing
5. The JSON results are returned to the AscendDev platform

## Test Format

Tests should be written using xUnit. Here's an example test template:

```csharp
using System;
using Xunit;

// User code will be inserted here
__USER_CODE__

public class Tests
{
    [Fact]
    public void Test_Add_TwoNumbers_ReturnsSum()
    {
        // Arrange
        var calculator = new Calculator();

        // Act
        int result = calculator.Add(2, 3);

        // Assert
        Assert.Equal(5, result);
    }
}
```

## Custom Configuration

You can customize the test execution by providing a `test-config.json` file with the following format:

```json
{
  "timeout": 10000
}
```

This will set the test timeout to 10 seconds.
