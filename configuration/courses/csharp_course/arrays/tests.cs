using System;
using Xunit;

public class ArrayTests
{
    [Fact]
    public void Test1()
    {
        var solution = new ArraySolution();
        var input = new int[] { 1, 2, 3 };
        var expected = new int[] { 3, 2, 1 };
        var result = solution.ReverseArray(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Test2()
    {
        var solution = new ArraySolution();
        var input = new int[] { 4, 5, 6 };
        var expected = new int[] { 6, 5, 4 };
        var result = solution.ReverseArray(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Test3()
    {
        var solution = new ArraySolution();
        var input = new int[] { 7, 8, 9 };
        var expected = new int[] { 9, 8, 7 };
        var result = solution.ReverseArray(input);
        Assert.Equal(expected, result);
    }
}