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
        public LiteDbSet<Person> People { get; [UsedImplicitly] private set; }

        private static void ConfigureModel(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Person>();
        }
    }

    [Fact]
    public void InsertRecord_WithValidInput_ShouldInsertRecord()
    {
        string path = Path.GetTempFileName();
        Person person = new() { Name = "Test Person" };

        using (DbContext context = new(path))
        {
            context.People.Insert(person);
        }
        using LiteDatabase liteDb = new(path);
        Person result = liteDb.GetCollection<Person>("Person").FindById(person.Id);
        Assert.NotNull(result);
        Assert.Equal("Test Person", result.Name);
    }

    [Fact]
    public void DeleteRecord_WithValidInput_ShouldDeleteRecord()
    {
        string path = Path.GetTempFileName();
        Person person = new() { Name = "Test Person" };

        using LiteDatabase liteDb = new($"Filename={path};Connection=shared");
        ILiteCollection<Person> people = liteDb.GetCollection<Person>("Person");
        people.Insert(person);

        using (DbContext context = new($"Filename={path};Connection=shared"))
        {
            bool deleted = context.People.Delete(person.Id);
            Assert.True(deleted);
        }
        Assert.Null(people.FindById(person.Id));
    }

    [Fact]
    public void UpdateRecord_WithValidInput_ShouldUpdateRecord()
    {
        string path = Path.GetTempFileName();
        Person person = new() { Name = "Old Name" };

        using LiteDatabase liteDb = new($"Filename={path};Connection=shared");
        ILiteCollection<Person> people = liteDb.GetCollection<Person>("Person");
        people.Insert(person);

        using (DbContext context = new($"Filename={path};Connection=shared"))
        {
            person.Name = "New Name";
            bool updated = context.People.Update(person);

            Assert.True(updated);
        }

        Person result = people.FindById(person.Id);
        Assert.NotNull(result);
        Assert.Equal("New Name", result.Name);
    }

    [Fact]
    public void UpsertNewRecord_WithValidData_ShouldInsertRecord()
    {
        string path = Path.GetTempFileName();
        Person person = new() { Name = "Person 1" };

        using (DbContext context = new(path))
        {
            bool result = context.People.Upsert(person);
            Assert.True(result);
        }

        using LiteDatabase liteDb = new(path);
        Person insertedPerson = liteDb.GetCollection<Person>("Person").FindById(person.Id);
        Assert.NotNull(insertedPerson);
        Assert.Equal("Person 1", insertedPerson.Name);
    }

    [Fact]
    public void UpsertExistingRecord_WithValidData_ShouldUpdateRecord()
    {
        string path = Path.GetTempFileName();
        Person person = new() { Name = "Person 1" };

        using LiteDatabase liteDb = new($"Filename={path};Connection=shared");
        ILiteCollection<Person> people = liteDb.GetCollection<Person>("Person");
        people.Insert(person);

        using (DbContext context = new($"Filename={path};Connection=shared"))
        {
            person.Name = "Updated Person 1";
            bool result = context.People.Upsert(person);
            // somewhat undocumented, Upsert returns true if the record was inserted or false if it was updated
            // https://github.com/litedb-org/LiteDB/issues/1410#issuecomment-577352959
            Assert.False(result);
        }

        Person updatedPerson = people.FindById(person.Id);
        Assert.NotNull(updatedPerson);
        Assert.Equal("Updated Person 1", updatedPerson.Name);
    }

    [Fact]
    public void FindAll_ShouldReturnSuccess()
    {
        string path = Path.GetTempFileName();

        using (LiteDatabase liteDb = new(path))
        {
            ILiteCollection<Person> people = liteDb.GetCollection<Person>("Person");
            people.Insert(new Person { Name = "Person 1" });
            people.Insert(new Person { Name = "Person 2" });
        }

        using DbContext context = new(path);
        IEnumerable<Person> results = context.People.FindAll();

        Assert.Equal(2, results.Count());
    }

    [Fact]
    public void FindById_WithValidData_ShouldReturnRecord()
    {
        string path = Path.GetTempFileName();
        Person person = new() { Name = "Person 1" };
        using (LiteDatabase db = new(path))
        {
            ILiteCollection<Person> people = db.GetCollection<Person>("Person");
            people.Insert(person);
        }

        using DbContext context = new(path);
        Person retrievedPerson = context.People.FindById(person.Id);

        Assert.NotNull(retrievedPerson);
        Assert.Equal(person.Id, retrievedPerson.Id);
        Assert.Equal(person.Name, retrievedPerson.Name);
    }

    [Fact]
    public void FindById_WithNonExistentId_ShouldReturnNull()
    {
        string path = Path.GetTempFileName();
        using DbContext context = new(path);

        Person result = context.People.FindById(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public void QueryRecord_WithValidInput_ShouldReturnRecord()
    {
        string path = Path.GetTempFileName();
        using (LiteDatabase liteDb = new(path))
        {
            ILiteCollection<Person> people = liteDb.GetCollection<Person>("Person");
            people.Insert(new Person { Name = "Person 1" });
            people.Insert(new Person { Name = "Person 2" });
        }

        using DbContext context = new(path);
        ILiteQueryable<Person> query = context.People.Query().Where(x => x.Name.Contains('1'));
        List<Person> results = query.ToList();

        Assert.Single(results);
        Assert.Equal("Person 1", results[0].Name);
    }
}