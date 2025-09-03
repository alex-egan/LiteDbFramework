using Xunit;
using LiteDB;
using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestPlatform.CrossPlatEngine.Adapter;
using static LiteDbFramework.Tests.LiteDbRefTests;

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

        private class DbContext(string connectionString) : LiteDbContext(connectionString, ConfigureModel)
        {
            public LiteDbSet<Person> People { get; private set; }

            private static void ConfigureModel(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Person>(col => col.EnsureIndex(x => x.Id));
            }
        }

        [Fact]
        public void InsertRecord_WithValidInput_ShouldInsertRecord()
        {
            var path = Path.GetTempFileName();
            using var context = new DbContext(path);

            var person = new Person { Name = "Test Person" };
            context.People.Insert(person);

            var result = context.People.FindById(person.Id);

            Assert.NotNull(result);
            Assert.Equal("Test Person", result.Name);
        }

        [Fact]
        public void DeleteRecord_WithValidInput_ShouldDeleteRecord()
        {
            var path = Path.GetTempFileName();
            using var context = new DbContext(path);

            var person = new Person { Name = "Test Person" };
            context.People.Insert(person);

            var deleted = context.People.Delete(person.Id);

            Assert.True(deleted);
            Assert.Null(context.People.FindById(person.Id));
        }

        [Fact]
        public void UpdateRecord_WithValidInput_ShouldUpdateRecord()
        {
            var path = Path.GetTempFileName();
            using var context = new DbContext(path);

            var person = new Person { Name = "Old Name" };
            context.People.Insert(person);

            person.Name = "New Name";
            var updated = context.People.Update(person);

            Assert.True(updated);
            var result = context.People.FindById(person.Id);
            Assert.NotNull(result);
            Assert.Equal("New Name", result.Name);
        }

        [Fact]
        public void UpsertNewRecord_WithValidData_ShouldInsertRecord()
        {
            var path = Path.GetTempFileName();
            using var context = new DbContext(path);

            // Insert a new entity
            var parent = new Person { Name = "Person A" };
            context.People.Upsert(parent);

            // Verify the entity was inserted
            var insertedPerson = context.People.FindById(parent.Id);
            Assert.NotNull(insertedPerson);
            Assert.Equal("Person A", insertedPerson.Name);
        }

        [Fact]
        public void UpsertExistingRecord_WithValidData_ShouldUpdateRecord()
        {
            var path = Path.GetTempFileName();
            using var context = new DbContext(path);

            // Insert a new entity
            var parent = new Person { Name = "Person A" };
            context.People.Upsert(parent);

            // Update the existing entity
            parent.Name = "Updated Person A";
            context.People.Upsert(parent);

            // Verify the entity was updated
            var updatedPerson = context.People.FindById(parent.Id);
            Assert.NotNull(updatedPerson);
            Assert.Equal("Updated Person A", updatedPerson.Name);
        }

        [Fact]
        public void FindAll_ShouldReturnSuccess()
        {
            var path = Path.GetTempFileName();
            using var context = new DbContext(path);

            context.People.Insert(new Person { Name = "Person 1" });
            context.People.Insert(new Person { Name = "Person 2" });

            var results = context.People.FindAll();

            Assert.Equal(2, results.Count());
        }

        [Fact]
        public void FindById_WithValidData_ShouldReturnRecord()
        {
            var path = Path.GetTempFileName();
            var person = new Person();
            using (var db = new LiteDatabase(path))
            {
                // Use standard LiteDB commands to insert a record
                var people = db.GetCollection<Person>("Person");
                person = new Person { Name = "Person A" };
                people.Insert(person);
            }

            // Use the project's FindById method to retrieve the record
            using var context = new DbContext(path);
            var retrievedParent = context.People.FindById(person.Id);

            // Verify the retrieved entity
            Assert.NotNull(retrievedParent);
            Assert.Equal(person.Id, retrievedParent.Id);
            Assert.Equal("Person A", retrievedParent.Name);
        }
        
        [Fact]
        public void FindById_WithNonExistentId_ShouldReturnNull()
        {
            var path = Path.GetTempFileName();
            using var context = new DbContext(path);

            var result = context.People.FindById(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public void QueryRecord_WithValidInput_ShouldReturnRecord()
        {
            var path = Path.GetTempFileName();
            using var context = new DbContext(path);

            context.People.Insert(new Person { Name = "Person A" });
            context.People.Insert(new Person { Name = "Person B" });

            var query = context.People.Query().Where(x => x.Name.Contains("A"));
            var results = query.ToList();

            Assert.Single(results);
            Assert.Equal("Person A", results[0].Name);
        }
    }
}