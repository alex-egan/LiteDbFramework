namespace LiteDbFramework.Tests;

public class LiteDbContextTests
{
    private class TestContext(string path) : LiteDbContext(path, _ => { });

    [Fact]
    public void InitializeContext_ValidInput_ShouldReturnSuccess()
    {
        string path = Path.GetTempFileName();
        using TestContext context = new(path);
        Assert.NotNull(context);
    }
}