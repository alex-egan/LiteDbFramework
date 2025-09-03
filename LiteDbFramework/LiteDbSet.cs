namespace LiteDbFramework;

public class LiteDbSet<T> where T : class
{
    private readonly ILiteCollection<T> _collection;
    private readonly IEnumerable<PropertyInfo> _refProps;

    public LiteDbSet(LiteDatabase db)
    {
        _collection = db.GetCollection<T>(typeof(T).Name);
        _refProps = typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetCustomAttribute<BsonRefAttribute>() != null);
    }

    public bool Delete(BsonValue id) => _collection.Delete(id);
    public IEnumerable<T> FindAll() => _collection.FindAll();
    public T FindById(BsonValue id) => _collection.FindById(id);
    public BsonValue Insert(T entity) => _collection.Insert(entity);
    public ILiteQueryable<T> Query() => _collection.Query();
    public bool Update(T entity) => _collection.Update(entity);
    public bool Upsert(T entity) => _collection.Upsert(entity);

    public LiteDbReferenceSet<T> WithReferences => new(_collection, _refProps);
//    public IEnumerable<T> FindAllWithReferences() => WithReferences().FindAll();
//    public T FindByIdWithReferences(BsonValue id) => WithReferences().FindById(id);
    //public ILiteCollection<T> WithReferences()
    //{
    //    ILiteCollection<T> collection = _collection;

    //    foreach (PropertyInfo prop in _refProps)
    //    {
    //        collection = collection.Include($"$.{prop.Name}");
    //    }

    //    return collection;
    //}
}