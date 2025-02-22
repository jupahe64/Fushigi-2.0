using Fushigi.Data.BymlSerialization;

namespace Fushigi.Data.Files.GymlTypes;

public abstract class GymlFile<T> : SerializableBymlObject<T>
    where T : GymlFile<T>, IGymlType, new()
{
    public T? Parent => _parent;
    
    private T? _parent;
    private GymlRef<T>? _parentGymlRef;

    protected override void Serialization<TContext>(TContext ctx)
    {
        ctx.Set(GYML_REF<T>(), ref _parentGymlRef!, "$parent", optional: true);
    }
    
    internal GymlRef<T>? ParentGymlRef => _parentGymlRef;

    internal void SetParent(T parent)
    {
        _parent = parent;
    }
}