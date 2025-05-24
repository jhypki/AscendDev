# Basic Math Operations in C#

In this lesson, you'll learn how to implement basic math operations in C#.

## Introduction

C# provides all the standard arithmetic operators that you would expect from a modern programming language. These include:

- Addition (`+`)
- Subtraction (`-`)
- Multiplication (`*`)
- Division (`/`)
- Modulus (`%`)

## Creating a Calculator Class

In this exercise, you'll create a simple `Calculator` class that implements these basic operations.

```csharp
public class Calculator
{
    public int Add(int a, int b)
    {
        return a + b;
    }

    public int Subtract(int a, int b)
    {
        return a - b;
    }

    // Implement Multiply and Divide methods
}
```

## Your Task

Complete the `Calculator` class by implementing the following methods:

1. `Multiply(int a, int b)` - Returns the product of `a` and `b`
2. `Divide(int a, int b)` - Returns the result of `a` divided by `b`
   - Make sure to handle division by zero by throwing a `DivideByZeroException`

## Testing Your Code

Your code will be tested with various inputs to ensure that:

- The `Add` method correctly adds two numbers
- The `Subtract` method correctly subtracts the second number from the first
- The `Multiply` method correctly multiplies two numbers
- The `Divide` method correctly divides the first number by the second
- The `Divide` method throws a `DivideByZeroException` when the second number is zero

## Example Usage

```csharp
var calculator = new Calculator();
int sum = calculator.Add(5, 3);        // Returns 8
int difference = calculator.Subtract(5, 3); // Returns 2
int product = calculator.Multiply(5, 3);    // Returns 15
int quotient = calculator.Divide(6, 3);     // Returns 2
```

Good luck!
