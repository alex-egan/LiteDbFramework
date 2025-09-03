using Xunit;
using LiteDB;
using System;
using System.IO;

namespace LiteDbFramework.Tests;

public class LiteDbRefTests
{
    public class Parent
    {
        [BsonId]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
    }

    public class Child
    {
        [BsonId]
        public Guid Id { get; set; } = Guid.NewGuid();

        [BsonRef("Parent")]
        public Parent Reference { get; set; }
    }

    private class DbContext(string connectionString) : LiteDbContext(connectionString, ConfigureModel)
    {
        public LiteDbSet<Parent> Parents { get; private set; }
        public LiteDbSet<Child> Children { get; private set; }

        private static void ConfigureModel(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Parent>(col => col.EnsureIndex(x => x.Id));
            modelBuilder.Entity<Child>(col => col.EnsureIndex(x => x.Id));
        }
    }

    [Fact]
    public void FindByIdWithReference_ShouldReturnSuccess()
    {
        var path = Path.GetTempFileName();
        var parent = new Parent { Name = "Parent 1" };
        var child = new Child { Reference = parent };

        using (var liteDb = new LiteDatabase(path))
        {
            var parents = liteDb.GetCollection<Parent>("Parent");
            var children = liteDb.GetCollection<Child>("Child");
            parents.Insert(parent);
            children.Insert(child);
        }

        using var context = new DbContext(path);
        var result = context.Children.WithReferences.FindById(child.Id);

        Assert.NotNull(result);
        Assert.NotNull(result.Reference);
        Assert.Equal("Parent 1", result.Reference.Name);
    }
}