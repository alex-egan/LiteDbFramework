namespace LiteDbFramework.Tests;

public class LiteDbContextTests
{
    private class TestContext(string path) : LiteDbContext(path, _ => { });

    [Fact]
    public void InitializeContext_ValidInput_ShouldReturnSuccess()
    {
        var path = Path.GetTempFileName();
        using var context = new TestContext(path);
        Assert.NotNull(context);
    }
}