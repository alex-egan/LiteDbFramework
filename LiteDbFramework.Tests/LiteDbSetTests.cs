using Xunit;
using LiteDB;
using System;
using System.IO;
using System.Linq;

namespace LiteDbFramework.Tests;

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
        var person = new Person { Name = "Test Person" };

        using (var context = new DbContext(path))
        {
            context.People.Insert(person);
        }
        using var liteDb = new LiteDatabase(path);
        var result = liteDb.GetCollection<Person>("Person").FindById(person.Id);
        Assert.NotNull(result);
        Assert.Equal("Test Person", result.Name);
    }

    [Fact]
    public void DeleteRecord_WithValidInput_ShouldDeleteRecord()
    {
        var path = Path.GetTempFileName();
        var person = new Person { Name = "Test Person" };

        using var liteDb = new LiteDatabase($"Filename={path};Connection=shared");
        var people = liteDb.GetCollection<Person>("Person");
        people.Insert(person);

        using (var context = new DbContext($"Filename={path};Connection=shared"))
        {
            var deleted = context.People.Delete(person.Id);
            Assert.True(deleted);
        }
        Assert.Null(people.FindById(person.Id));
    }

    [Fact]
    public void UpdateRecord_WithValidInput_ShouldUpdateRecord()
    {
        var path = Path.GetTempFileName();
        var person = new Person { Name = "Old Name" };

        using var liteDb = new LiteDatabase($"Filename={path};Connection=shared");
        var people = liteDb.GetCollection<Person>("Person");
        people.Insert(person);

        using (var context = new DbContext($"Filename={path};Connection=shared"))
        {
            person.Name = "New Name";
            var updated = context.People.Update(person);

            Assert.True(updated);
        }

        var result = people.FindById(person.Id);
        Assert.NotNull(result);
        Assert.Equal("New Name", result.Name);
    }

    [Fact]
    public void UpsertNewRecord_WithValidData_ShouldInsertRecord()
    {
        var path = Path.GetTempFileName();
        var person = new Person { Name = "Person 1" };

        using (var context = new DbContext(path))
        {
            var result = context.People.Upsert(person);
            Assert.True(result);
        }

        using var liteDb = new LiteDatabase(path);
        var insertedPerson = liteDb.GetCollection<Person>("Person").FindById(person.Id);
        Assert.NotNull(insertedPerson);
        Assert.Equal("Person 1", insertedPerson.Name);
    }

    [Fact]
    public void UpsertExistingRecord_WithValidData_ShouldUpdateRecord()
    {
        var path = Path.GetTempFileName();
        var person = new Person { Name = "Person 1" };

        using var liteDb = new LiteDatabase($"Filename={path};Connection=shared");
        var people = liteDb.GetCollection<Person>("Person");
        people.Insert(person);

        using (var context = new DbContext($"Filename={path};Connection=shared"))
        {
            person.Name = "Updated Person 1";
            var result = context.People.Upsert(person);
            // somewhat undocumented, Upsert returns true if the record was inserted or false if it was updated
            // https://github.com/litedb-org/LiteDB/issues/1410#issuecomment-577352959
            Assert.False(result);
        }

        var updatedPerson = people.FindById(person.Id);
        Assert.NotNull(updatedPerson);
        Assert.Equal("Updated Person 1", updatedPerson.Name);
    }

    [Fact]
    public void FindAll_ShouldReturnSuccess()
    {
        var path = Path.GetTempFileName();

        using (var liteDb = new LiteDatabase(path))
        {
            var people = liteDb.GetCollection<Person>("Person");
            people.Insert(new Person { Name = "Person 1" });
            people.Insert(new Person { Name = "Person 2" });
        }

        using var context = new DbContext(path);
        var results = context.People.FindAll();

        Assert.Equal(2, results.Count());
    }

    [Fact]
    public void FindById_WithValidData_ShouldReturnRecord()
    {
        var path = Path.GetTempFileName();
        var person = new Person { Name = "Person 1" };
        using (var db = new LiteDatabase(path))
        {
            var people = db.GetCollection<Person>("Person");
            people.Insert(person);
        }

        using var context = new DbContext(path);
        var retrievedPerson = context.People.FindById(person.Id);

        Assert.NotNull(retrievedPerson);
        Assert.Equal(person.Id, retrievedPerson.Id);
        Assert.Equal(person.Name, retrievedPerson.Name);
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
        using (var liteDb = new LiteDatabase(path))
        {
            var people = liteDb.GetCollection<Person>("Person");
            people.Insert(new Person { Name = "Person 1" });
            people.Insert(new Person { Name = "Person 2" });
        }

        using var context = new DbContext(path);
        var query = context.People.Query().Where(x => x.Name.Contains('1'));
        var results = query.ToList();

        Assert.Single(results);
        Assert.Equal("Person 1", results[0].Name);
    }
}