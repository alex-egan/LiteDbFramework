using System.Linq.Expressions;

namespace LiteDbFramework;

/// <summary>
/// Represents a set of entities in a LiteDB collection with included references.
/// </summary>
/// <typeparam name="T">The type of the entity.</typeparam>
[PublicAPI]
public class LiteDbReferenceSet<T>
{
    private readonly ILiteCollection<T> _collection;

    /// <summary>
    /// Initializes a new instance of the <see cref="LiteDbReferenceSet{T}"/> class
    /// with the specified base collection and reference properties.
    /// </summary>
    /// <param name="baseCollection">The base LiteDB collection.</param>
    /// <param name="refProps">The reference properties to include.</param>
    public LiteDbReferenceSet(ILiteCollection<T> baseCollection, IEnumerable<PropertyInfo> refProps)
    {
        foreach (PropertyInfo prop in refProps)
        {
            baseCollection = baseCollection.Include($"$.{prop.Name}");
        }

        _collection = baseCollection;
    }

    /// <summary>
    /// Retrieves all entities in the collection, including references.
    /// </summary>
    /// <returns>An IEnumerable of all entities.</returns>
    public IEnumerable<T> FindAll() => _collection.FindAll();

    /// <summary>
    /// Retrieves all entities in the collection that satisfy the specified condition, including references
    /// </summary>
    /// <param name="predicate">The condition to be met</param>
    /// <returns>An IEnumerable of all matching entities</returns>
    public IEnumerable<T> Find(Expression<Func<T, bool>> predicate) => _collection.Find(predicate);

    /// <summary>
    /// Finds an entity by its ID, including references.
    /// </summary>
    /// <param name="id">The ID of the entity to find.</param>
    /// <returns>The entity if found; otherwise, null.</returns>
    public T? FindById(BsonValue id) => _collection.FindById(id);
}
