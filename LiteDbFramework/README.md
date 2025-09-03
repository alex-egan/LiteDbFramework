# Pepperdine.LiteDbFramework

An Entity Framework–inspired abstraction layer for [LiteDB](https://www.litedb.org), built to simplify data access and modeling in .NET applications.

## Features

- `LiteDbContext` and `LiteDbSet<T>` interfaces for working with collections
- Support for `[BsonRef]` and automatic reference resolution
- `ModelBuilder` API for centralized model configuration

## Usage

### 1. Define Your Models

```csharp
public class User
{
    [BsonId]
    public Guid Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
}

public class Post
{
    [BsonId]
    public Guid Id { get; set; }

    [BsonRef]
    public User Author { get; set; }
}
```

---

### 2. Create a DbContext

```csharp
public class AppDbContext(string connectionString) : LiteDbContext(connectionString, ConfigureModel)
{
    public LiteDbSet<User> Users { get; private set; }
    public LiteDbSet<Post> Posts { get; private set; }

    private static void ConfigureModel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>();
        modelBuilder.Entity<Post>();
    }
}
```

---

### 3. Perform CRUD Operations

```csharp
using AppDbContext db = new AppDbContext("MyData.db");

// Insert
User user = new User { Email = "example@example.com", Name = "Xavier Example" };
db.Users.Insert(user);

// Query
User? found = db.Users.FindById(user.Id);
Console.WriteLine(found?.Email);

// Update
found.Name = "Xavier E.";
db.Users.Upsert(found);

// Delete
db.Users.Remove(found.Id);
```

---

### 4. Using `WithReferences` for Relationships

```csharp
// Setup context and insert post
Post post = new Post { Author = user };
db.Posts.Insert(post);

// Fetch with reference included
Post? result = db.GetCollection<Post>()
    .WithReferences
    .FindById(post.Id);

Console.WriteLine(result.Author?.Name);
```

## License

This project is licensed under the MIT License.

## Dependencies

This library depends on:

- [LiteDB](https://github.com/litedb-org/LiteDB) – MIT Licensed
