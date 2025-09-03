namespace LiteDbFramework;

/// <summary>
/// Provides functionality to configure LiteDB collections.
/// </summary>
/// <param name="db">The LiteDatabase instance to operate on.</param>
[PublicAPI]
public class ModelBuilder(LiteDatabase db)
{
    /// <summary>
    /// Configures a LiteDB collection for the specified entity type.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <param name="configure">An optional action to configure the LiteDB collection.</param>
    public void Entity<T>(Action<ILiteCollection<T>>? configure = null) where T : class
    {
        ILiteCollection<T> collection = db.GetCollection<T>(typeof(T).Name);
        configure?.Invoke(collection);
    }
}