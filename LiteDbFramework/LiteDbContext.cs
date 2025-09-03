namespace LiteDbFramework;

/// <summary>
/// Represents the base class for a LiteDB context, providing access to database sets and configuration.
/// </summary>
[PublicAPI]
public abstract class LiteDbContext : IDisposable
{
    private readonly LiteDatabase _db;

    /// <summary>
    /// Initializes a new instance of the <see cref="LiteDbContext"/> class.
    /// </summary>
    /// <param name="connectionString">The connection string for the LiteDB database.</param>
    /// <param name="configureModel">An action to configure the database model.</param>
    protected LiteDbContext(string connectionString, Action<ModelBuilder> configureModel)
    {
        _db = new LiteDatabase(connectionString);
        ModelBuilder modelBuilder = new(_db);
        configureModel(modelBuilder);
        InitializeDbSets();
    }

    private void InitializeDbSets()
    {
        IEnumerable<PropertyInfo> dbSetProps = GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(p => p.PropertyType.IsGenericType &&
                        p.PropertyType.GetGenericTypeDefinition() == typeof(LiteDbSet<>));

        foreach (PropertyInfo prop in dbSetProps)
        {
            Type entityType = prop.PropertyType.GetGenericArguments()[0];

            object? dbSet = Activator.CreateInstance(
                typeof(LiteDbSet<>).MakeGenericType(entityType),
                _db);

            prop.SetValue(this, dbSet);
        }
    }

    /// <summary>
    /// Releases all resources used by the <see cref="LiteDbContext"/>.
    /// </summary>
    public void Dispose()
    {
        _db.Dispose();
        GC.SuppressFinalize(this);
    }
}