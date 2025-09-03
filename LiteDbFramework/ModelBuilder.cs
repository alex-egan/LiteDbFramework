namespace LiteDbFramework;

public class ModelBuilder
{
    private readonly LiteDatabase _db;

    public ModelBuilder(LiteDatabase db)
    {
        _db = db;
    }

    public void Entity<T>(Action<ILiteCollection<T>> configure) where T : class
    {
        ILiteCollection<T> collection = _db.GetCollection<T>(typeof(T).Name);
        configure(collection);
    }
}