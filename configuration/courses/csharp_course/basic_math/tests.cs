using System;
using Xunit;

// User code will be inserted here
__USER_CODE__

public class CalculatorTests
{
    [Fact]
    public void Test_Add_TwoPositiveNumbers_ReturnsSum()
    {
        // Arrange
        var calculator = new Calculator();

        // Act
        int result = calculator.Add(5, 3);

        // Assert
        Assert.Equal(8, result);
    }

    [Fact]
    public void Test_Add_NegativeNumbers_ReturnsSum()
    {
        // Arrange
        var calculator = new Calculator();

        // Act
        int result = calculator.Add(-5, -3);

        // Assert
        Assert.Equal(-8, result);
    }

    [Fact]
    public void Test_Subtract_PositiveNumbers_ReturnsDifference()
    {
        // Arrange
        var calculator = new Calculator();

        // Act
        int result = calculator.Subtract(5, 3);

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public void Test_Subtract_NegativeNumbers_ReturnsDifference()
    {
        // Arrange
        var calculator = new Calculator();

        // Act
        int result = calculator.Subtract(-5, -3);

        // Assert
        Assert.Equal(-2, result);
    }

    [Fact]
    public void Test_Multiply_PositiveNumbers_ReturnsProduct()
    {
        // Arrange
        var calculator = new Calculator();

        // Act
        int result = calculator.Multiply(5, 3);

        // Assert
        Assert.Equal(15, result);
    }

    [Fact]
    public void Test_Multiply_NegativeNumbers_ReturnsProduct()
    {
        // Arrange
        var calculator = new Calculator();

        // Act
        int result = calculator.Multiply(-5, -3);

        // Assert
        Assert.Equal(15, result);
    }

    [Fact]
    public void Test_Multiply_ZeroAndNumber_ReturnsZero()
    {
        // Arrange
        var calculator = new Calculator();

        // Act
        int result = calculator.Multiply(0, 3);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void Test_Divide_PositiveNumbers_ReturnsQuotient()
    {
        // Arrange
        var calculator = new Calculator();

        // Act
        int result = calculator.Divide(6, 3);

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public void Test_Divide_NegativeNumbers_ReturnsQuotient()
    {
        // Arrange
        var calculator = new Calculator();

        // Act
        int result = calculator.Divide(-6, -3);

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public void Test_Divide_ByZero_ThrowsDivideByZeroException()
    {
        // Arrange
        var calculator = new Calculator();

        // Act & Assert
        Assert.Throws<DivideByZeroException>(() => calculator.Divide(5, 0));
    }
}