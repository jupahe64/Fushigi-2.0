using Fushigi.Data.BymlSerialization;

namespace Fushigi.Data.Files.GymlTypes;

public abstract class GymlFile<T> : SerializableBymlObject<T>
    where T : GymlFile<T>, new()
{
    public T? Parent => _parent;
    
    private T? _parent;
    private GymlRef? _parentGymlRef;

    protected override void Serialization<TContext>(TContext ctx)
    {
        ctx.Set(GYML_REF, ref _parentGymlRef!, "$parent", optional: true);
    }
    
    internal GymlRef? ParentGymlRef => _parentGymlRef;

    internal void SetParent(T parent)
    {
        _parent = parent;
    }
}