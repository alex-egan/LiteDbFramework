namespace LiteDbFramework;

public class LiteDbSet<T>(LiteDatabase db) where T : class
{
    private readonly ILiteCollection<T> _collection = db.GetCollection<T>(typeof(T).Name);
    private readonly IEnumerable<PropertyInfo> _refProps = typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetCustomAttribute<BsonRefAttribute>() != null);

    public bool Delete(BsonValue id) => _collection.Delete(id);
    public IEnumerable<T> FindAll() => _collection.FindAll();
    public T FindById(BsonValue id) => _collection.FindById(id);
    public BsonValue Insert(T entity) => _collection.Insert(entity);
    public ILiteQueryable<T> Query() => _collection.Query();
    public bool Update(T entity) => _collection.Update(entity);
    public bool Upsert(T entity) => _collection.Upsert(entity);

    public LiteDbReferenceSet<T> WithReferences => new(_collection, _refProps);
}