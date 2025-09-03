namespace LiteDbFramework;

public class LiteDbReferenceSet<T>
{
    private readonly ILiteCollection<T> _collection;

    public LiteDbReferenceSet(ILiteCollection<T> baseCollection, IEnumerable<PropertyInfo> refProps)
    {
        foreach (var prop in refProps)
        {
            baseCollection = baseCollection.Include($"$.{prop.Name}");
        }

        _collection = baseCollection;
    }

    public IEnumerable<T> FindAll() => _collection.FindAll();
    public T? FindById(BsonValue id) => _collection.FindById(id);
}
