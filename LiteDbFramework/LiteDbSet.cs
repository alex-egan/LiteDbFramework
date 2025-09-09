using System.Linq.Expressions;

namespace LiteDbFramework;

/// <summary>
/// Represents a set of entities in a LiteDB database.
/// </summary>
/// <typeparam name="T">The type of the entity.</typeparam>
/// <param name="db">The LiteDatabase instance to operate on.</param>
[PublicAPI]
public class LiteDbSet<T>(LiteDatabase db) where T : class
{
    private readonly ILiteCollection<T> _collection = db.GetCollection<T>(typeof(T).Name);
    private readonly IEnumerable<PropertyInfo> _refProps = typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetCustomAttribute<BsonRefAttribute>() != null);

    /// <summary>
    /// Deletes an entity by its ID.
    /// </summary>
    /// <param name="id">The ID of the entity to delete.</param>
    /// <returns>True if the entity was deleted; otherwise, false.</returns>
    public bool Delete(BsonValue id) => _collection.Delete(id);

    /// <summary>
    /// Retrieves all entities in the collection.
    /// </summary>
    /// <returns>An IEnumerable of all entities.</returns>
    public IEnumerable<T> FindAll() => _collection.FindAll();

    /// <summary>
    /// Retrieves all entities in the collection that satisfy the specified condition
    /// </summary>
    /// <param name="predicate">The condition to be met</param>
    /// <returns>An IEnumerable of all matching entities</returns>
    public IEnumerable<T> Find(Expression<Func<T, bool>> predicate) => _collection.Find(predicate);

    /// <summary>
    /// Finds an entity by its ID.
    /// </summary>
    /// <param name="id">The ID of the entity to find.</param>
    /// <returns>The entity if found; otherwise, null.</returns>
    public T FindById(BsonValue id) => _collection.FindById(id);

    /// <summary>
    /// Inserts a new entity into the collection.
    /// </summary>
    /// <param name="entity">The entity to insert.</param>
    /// <returns>The ID of the inserted entity.</returns>
    public BsonValue Insert(T entity) => _collection.Insert(entity);

    /// <summary>
    /// Creates a queryable interface for the collection.
    /// </summary>
    /// <returns>An ILiteQueryable for the collection.</returns>
    public ILiteQueryable<T> Query() => _collection.Query();

    /// <summary>
    /// Updates an existing entity in the collection.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <returns>True if the entity was updated; otherwise, false.</returns>
    public bool Update(T entity) => _collection.Update(entity);

    /// <summary>
    /// Inserts or updates an entity in the collection.
    /// </summary>
    /// <param name="entity">The entity to upsert.</param>
    /// <returns>True if the entity was inserted; false if the entity was updated.</returns>
    public bool Upsert(T entity) => _collection.Upsert(entity);

    /// <summary>
    /// Provides access to reference-related operations for the collection.
    /// </summary>
    public LiteDbReferenceSet<T> WithReferences => new(_collection, _refProps);
}