using Fushigi.Data.BymlSerialization;

namespace Fushigi.Data.Files.GymlTypes;

public abstract class GymlFile<T> : SerializableBymlObject<T>
    where T : GymlFile<T>, new()
{
    public T? Parent => _parent;
    
    private T? _parent;
    private string? _parentGymlRefString;
    protected override void Deserialize(Deserializer d)
    {
        d.SetString(ref _parentGymlRefString!, "$parent", optional: true);
    }
    
    protected override void Serialize(Serializer d)
    {
        d.SetString(ref _parentGymlRefString!, "$parent", optional: true);
    }
    
    internal string? ParentGymlRefString => _parentGymlRefString;

    internal void SetParent(T parent)
    {
        _parent = parent;
    }
}