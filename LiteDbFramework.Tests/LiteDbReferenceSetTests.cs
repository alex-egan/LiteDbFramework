namespace LiteDbFramework.Tests;

public class LiteDbReferenceSetTests
{
    public class Parent
    {
        [BsonId]
        [UsedImplicitly]
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class Child
    {
        [BsonId]
        public Guid Id { get; set; }

        [BsonRef]
        public Parent ParentReference { get; set; }
    }

    public class Building
    {
        [BsonId]
        public Guid Id { get; set; }
        public string Name { get; set; }
        [BsonRef]
        public List<Room> Rooms { get; set; } = [];
    }

    public class Room
    {
        [BsonId]
        [UsedImplicitly]
        public Guid Id { get; set; }
        public string Number { get; set; }
    }

    private class DbContext(string connectionString) : LiteDbContext(connectionString, ConfigureModel)
    {
        public LiteDbSet<Parent> Parents { get; [UsedImplicitly] private set; }
        public LiteDbSet<Child> Children { get; [UsedImplicitly] private set; }
        public LiteDbSet<Building> Buildings { get; [UsedImplicitly] private set; }
        public LiteDbSet<Room> Rooms { get; [UsedImplicitly] private set; }

        private static void ConfigureModel(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Parent>();
            modelBuilder.Entity<Child>();
            modelBuilder.Entity<Building>();
            modelBuilder.Entity<Room>();
        }
    }

    [Fact]
    public void InsertRecordWithReference_WithValidInput_ShouldReturnSuccess()
    {
        string path = Path.GetTempFileName();
        Parent parent = new() { Name = "Parent 1" };
        Child child = new() { ParentReference = parent };

        using (DbContext dbContext = new(path))
        {
            dbContext.Parents.Insert(parent);
            dbContext.Children.Insert(child);
        }

        using LiteDatabase liteDb = new(path);
        Child result = liteDb.GetCollection<Child>("Child").Include(x => x.ParentReference).FindById(child.Id);

        Assert.NotNull(result);
        Assert.NotNull(result.ParentReference);
        Assert.Equal("Parent 1", result.ParentReference.Name);
    }

    [Fact]
    public void FindByIdWithReference_ShouldReturnSuccess()
    {
        string path = Path.GetTempFileName();
        Parent parent = new() { Name = "Parent 1" };
        Child child = new() { ParentReference = parent };

        using (LiteDatabase liteDb = new(path))
        {
            ILiteCollection<Parent> parents = liteDb.GetCollection<Parent>("Parent");
            ILiteCollection<Child> children = liteDb.GetCollection<Child>("Child");
            parents.Insert(parent);
            children.Insert(child);
        }

        using DbContext context = new(path);
        Child result = context.Children.WithReferences.FindById(child.Id);

        Assert.NotNull(result);
        Assert.NotNull(result.ParentReference);
        Assert.Equal("Parent 1", result.ParentReference.Name);
    }

    [Fact]
    public void FindAllWithReference_ShouldReturnSuccess()
    {
        string path = Path.GetTempFileName();
        Parent parent1 = new() { Name = "Parent 1" };
        Child child1 = new() { ParentReference = parent1 };
        Parent parent2 = new() { Name = "Parent 2" };
        Child child2 = new() { ParentReference = parent2 };

        using (LiteDatabase liteDb = new(path))
        {
            ILiteCollection<Parent> parents = liteDb.GetCollection<Parent>("Parent");
            ILiteCollection<Child> children = liteDb.GetCollection<Child>("Child");
            parents.Insert(parent1);
            children.Insert(child1);
            parents.Insert(parent2);
            children.Insert(child2);
        }

        using DbContext context = new(path);
        List<Child> result = context.Children.WithReferences.FindAll().ToList();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        List<string> expectedNames = [parent1.Name, parent2.Name];
        List<string> actualNames = result.ConvertAll(r => r.ParentReference.Name);
        Assert.True(expectedNames.Order().SequenceEqual(actualNames.Order()));
    }

    [Fact]
    public void InsertRecordWithListReference_WithValidInput_ShouldReturnSuccess()
    {
        string path = Path.GetTempFileName();
        Room room1 = new() { Number = "101" };
        Room room2 = new() { Number = "102" };
        Building building = new()
        {
            Name = "Building 1",
            Rooms = [room1, room2]
        };

        using (DbContext dbContext = new(path))
        {
            dbContext.Rooms.Insert(room1);
            dbContext.Rooms.Insert(room2);
            dbContext.Buildings.Insert(building);
        }

        using LiteDatabase liteDb = new(path);
        Building result = liteDb.GetCollection<Building>("Building").Include(x => x.Rooms).FindById(building.Id);
        Assert.NotNull(result);
        Assert.NotNull(result.Rooms);
        Assert.Equal(2, result.Rooms.Count);
        Assert.Equal("101", result.Rooms[0].Number);
        Assert.Equal("102", result.Rooms[1].Number);
    }
}