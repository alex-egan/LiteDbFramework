using Xunit;
using LiteDbFramework;
using LiteDB;
using System;
using System.IO;

namespace LiteDbFramework.Tests
{
    public class LiteDbSetTests
    {
        public class Person
        {
            [BsonId]
            public Guid Id { get; set; } = Guid.NewGuid();
            public string Name { get; set; } = string.Empty;
        }

        private class PersonContext : LiteDbContext
        {
            public LiteDbSet<Person> People { get; private set; }

            public PersonContext(string path)
                : base(path, builder => { }) { }
        }

        [Fact]
        public void CanInsertAndRetrievePerson()
        {
            var path = Path.GetTempFileName();
            using var context = new PersonContext(path);

            var person = new Person { Name = "Ada Lovelace" };
            context.People.Insert(person);

            var result = context.People.FindById(person.Id);

            Assert.NotNull(result);
            Assert.Equal("Ada Lovelace", result.Name);
        }
    }

    public class LiteDbSetAdditionalTests
    {
        public class Product
        {
            [BsonId]
            public Guid Id { get; set; } = Guid.NewGuid();
            public string Name { get; set; } = string.Empty;
        }

        private class ProductContext : LiteDbContext
        {
            public LiteDbSet<Product> Products { get; private set; }

            public ProductContext(string path)
                : base(path, builder => { }) { }
        }

        [Fact]
        public void CanDeleteProduct()
        {
            var path = Path.GetTempFileName();
            using var context = new ProductContext(path);

            var product = new Product { Name = "Test Product" };
            context.Products.Insert(product);

            var deleted = context.Products.Delete(product.Id);

            Assert.True(deleted);
            Assert.Null(context.Products.FindById(product.Id));
        }

        [Fact]
        public void CanUpdateProduct()
        {
            var path = Path.GetTempFileName();
            using var context = new ProductContext(path);

            var product = new Product { Name = "Old Name" };
            context.Products.Insert(product);

            product.Name = "New Name";
            var updated = context.Products.Update(product);

            Assert.True(updated);
            var result = context.Products.FindById(product.Id);
            Assert.NotNull(result);
            Assert.Equal("New Name", result.Name);
        }

        [Fact]
        public void CanQueryProducts()
        {
            var path = Path.GetTempFileName();
            using var context = new ProductContext(path);

            context.Products.Insert(new Product { Name = "Product A" });
            context.Products.Insert(new Product { Name = "Product B" });

            var query = context.Products.Query().Where(x => x.Name.Contains("A"));
            var results = query.ToList();

            Assert.Single(results);
            Assert.Equal("Product A", results[0].Name);
        }
    }
}