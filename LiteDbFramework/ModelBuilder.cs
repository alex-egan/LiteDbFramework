namespace LiteDbFramework;

public class ModelBuilder(LiteDatabase db)
{
    public void Entity<T>(Action<ILiteCollection<T>> configure) where T : class
    {
        ILiteCollection<T> collection = db.GetCollection<T>(typeof(T).Name);
        configure(collection);
    }
}