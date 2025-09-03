using Xunit;
using LiteDbFramework;
using LiteDB;
using System.IO;

namespace LiteDbFramework.Tests
{
    public class LiteDbContextTests
    {
        private class TestContext : LiteDbContext
        {
            public TestContext(string path) : base(path, builder => { }) { }
        }

        [Fact]
        public void CanInitializeContext()
        {
            var path = Path.GetTempFileName();
            using var context = new TestContext(path);
            Assert.NotNull(context);
        }
    }
}