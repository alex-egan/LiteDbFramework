using Xunit;
using LiteDbFramework;
using LiteDB;
using System;
using System.IO;
using System.Linq; // Ensure System.Linq is imported for Count() extension method

namespace LiteDbFramework.Tests
{
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

        private class RefContext(string connectionString) : LiteDbContext(connectionString, ConfigureModel)
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
        public void CanIncludeBsonRefProperty()
        {
            var path = Path.GetTempFileName();
            using var context = new RefContext(path);

            var parent = new Parent { Name = "Parent A" };
            context.Parents.Insert(parent);

            var child = new Child { Reference = parent };
            context.Children.Insert(child);

            var result = context.Children.WithReferences.FindById(child.Id);

            Assert.NotNull(result);
            Assert.NotNull(result.Reference);
            Assert.Equal("Parent A", result.Reference.Name);
        }

        [Fact]
        public void CanFindAllOrders()
        {
            var path = Path.GetTempFileName();
            using var context = new RefContext(path);

            context.Parents.Insert(new Parent { Name = "Order 1" });
            context.Parents.Insert(new Parent { Name = "Order 2" });

            var results = context.Parents.FindAll();

            Assert.Equal(2, results.Count());
        }

        [Fact]
        public void FindByIdReturnsNullForNonExistentId()
        {
            var path = Path.GetTempFileName();
            using var context = new RefContext(path);

            var result = context.Parents.FindById(Guid.NewGuid());

            Assert.Null(result);
        }
    }
}