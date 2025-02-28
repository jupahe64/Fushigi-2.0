namespace Fushigi.Data.BymlSerialization;

/// <summary>
/// Implemented only by <see cref="Deserializer"/> and <see cref="Serializer"/>
/// </summary>
public interface ISerializationContext
{
    public void Set<TValue>(BymlConversion<TValue> conversion, ref TValue value, string key,
        bool optional = false);

    public void Set<TValue>(BymlConversion<TValue> conversion, ref TValue? value, string key,
        bool optional = false)
        where TValue : struct;

    public void SetArray<TItem>(ref List<TItem> value, string key, BymlConversion<TItem> conversion,
        bool optional = false);

    public void SetMap<TItem>(ref Dictionary<string, TItem> value, string key, BymlConversion<TItem> conversion,
        bool optional = false);
}