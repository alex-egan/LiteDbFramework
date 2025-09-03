namespace LiteDbFramework.Tests;

public class LiteDbReferenceSetTests
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
        public Parent ParentReference { get; set; }
    }

    public class Building
    {
        [BsonId]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        [BsonRef]
        public List<Room> Rooms { get; set; } = [];
    }

    public class Room
    {
        [BsonId]
        public Guid Id { get; set; } = Guid.NewGuid();
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
            modelBuilder.Entity<Parent>(col => col.EnsureIndex(x => x.Id));
            modelBuilder.Entity<Child>(col => col.EnsureIndex(x => x.Id));
            modelBuilder.Entity<Building>(col => col.EnsureIndex(x => x.Id));
            modelBuilder.Entity<Room>(col => col.EnsureIndex(x => x.Id));
        }
    }

    [Fact]
    public void InsertRecordWithReference_WithValidInput_ShouldReturnSuccess()
    {
        var path = Path.GetTempFileName();
        var parent = new Parent { Name = "Parent 1" };
        var child = new Child { ParentReference = parent };

        using (var dbContext = new DbContext(path))
        {
            dbContext.Parents.Insert(parent);
            dbContext.Children.Insert(child);
        }

        using var liteDb = new LiteDatabase(path);
        var result = liteDb.GetCollection<Child>("Child").Include(x => x.ParentReference).FindById(child.Id);

        Assert.NotNull(result);
        Assert.NotNull(result.ParentReference);
        Assert.Equal("Parent 1", result.ParentReference.Name);
    }

    [Fact]
    public void FindByIdWithReference_ShouldReturnSuccess()
    {
        var path = Path.GetTempFileName();
        var parent = new Parent { Name = "Parent 1" };
        var child = new Child { ParentReference = parent };

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
        Assert.NotNull(result.ParentReference);
        Assert.Equal("Parent 1", result.ParentReference.Name);
    }

    [Fact]
    public void InsertRecordWithListReference_WithValidInput_ShouldReturnSuccess()
    {
        var path = Path.GetTempFileName();
        var room1 = new Room { Number = "101" };
        var room2 = new Room { Number = "102" };
        var building = new Building
        {
            Name = "Building 1",
            Rooms = [ room1, room2 ]
        };

        using (var dbContext = new DbContext(path))
        {
            dbContext.Rooms.Insert(room1);
            dbContext.Rooms.Insert(room2);
            dbContext.Buildings.Insert(building);
        }

        using var liteDb = new LiteDatabase(path);
        var result = liteDb.GetCollection<Building>("Building").Include(x => x.Rooms).FindById(building.Id);
        Assert.NotNull(result);
        Assert.NotNull(result.Rooms);
        Assert.Equal(2, result.Rooms.Count);
        Assert.Equal("101", result.Rooms[0].Number);
        Assert.Equal("102", result.Rooms[1].Number);
    }
}