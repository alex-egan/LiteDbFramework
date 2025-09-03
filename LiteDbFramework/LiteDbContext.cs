namespace LiteDbFramework;

public abstract class LiteDbContext : IDisposable
{
    private readonly LiteDatabase _db;

    protected LiteDbContext(string connectionString, Action<ModelBuilder> configureModel)
    {
        _db = new LiteDatabase(connectionString);
        ModelBuilder modelBuilder = new(_db);
        configureModel?.Invoke(modelBuilder);
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

            object dbSet = Activator.CreateInstance(
                typeof(LiteDbSet<>).MakeGenericType(entityType),
                _db);

            prop.SetValue(this, dbSet);
        }
    }

    public void Dispose()
    {
        _db?.Dispose();
    }
}