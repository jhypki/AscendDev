# Basic Math Operations in Python

In this lesson, you'll learn how to implement basic math operations in Python.

## Introduction

Python provides all the standard arithmetic operators that you would expect from a modern programming language. These include:

- Addition (`+`)
- Subtraction (`-`)
- Multiplication (`*`)
- Division (`/`)
- Integer Division (`//`)
- Modulus (`%`)
- Exponentiation (`**`)

## Creating a Calculator Class

In this exercise, you'll create a simple `Calculator` class that implements these basic operations.

```python
class Calculator:
    def add(self, a, b):
        return a + b

    def subtract(self, a, b):
        return a - b

    # Implement multiply and divide methods
```

## Your Task

Complete the `Calculator` class by implementing the following methods:

1. `multiply(a, b)` - Returns the product of `a` and `b`
2. `divide(a, b)` - Returns the result of `a` divided by `b`
   - Make sure to handle division by zero by raising a `ZeroDivisionError`
3. `power(a, b)` - Returns `a` raised to the power of `b`

## Testing Your Code

Your code will be tested with various inputs to ensure that:

- The `add` method correctly adds two numbers
- The `subtract` method correctly subtracts the second number from the first
- The `multiply` method correctly multiplies two numbers
- The `divide` method correctly divides the first number by the second
- The `divide` method raises a `ZeroDivisionError` when the second number is zero
- The `power` method correctly raises the first number to the power of the second

## Example Usage

```python
calculator = Calculator()
sum_result = calculator.add(5, 3)        # Returns 8
difference = calculator.subtract(5, 3)    # Returns 2
product = calculator.multiply(5, 3)       # Returns 15
quotient = calculator.divide(6, 3)        # Returns 2.0
power_result = calculator.power(2, 3)     # Returns 8
```

Good luck!
