__USER_CODE__

public class ProgramTests
{
    [Fact]
    public void Test1()
    {
        Assert.Equal(4, Program.Square(2));
    }

    [Fact]
    public void Test2()
    {
        Assert.Equal(9, Program.Square(3));
    }

    [Fact]
    public void Test3()
    {
        Assert.Equal(16, Program.Square(4));
    }
}