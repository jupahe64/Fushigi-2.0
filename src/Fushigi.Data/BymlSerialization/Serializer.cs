using System.Diagnostics.CodeAnalysis;
using BymlLibrary;
using BymlLibrary.Nodes.Containers;

namespace Fushigi.Data.BymlSerialization;

public readonly struct Serializer : ISerializationContext
{
    public Serializer(IDictionary<string, Byml> targetMap)
    {
        _targetMap = targetMap;
    }

    private readonly IDictionary<string, Byml> _targetMap;
    
    public void Set<TValue>(BymlConversion<TValue> conversion, ref TValue value, string key, 
        bool optional = false)
    {
        if (value == null)
        {
            _targetMap.Remove(key);
            return;
        }

        _targetMap[key] = conversion.Serialize(value);
    }
    public void Set<TValue>(BymlConversion<TValue> conversion, ref TValue? value, string key, 
        bool optional = false)
        where TValue : struct
    {
        if (!value.HasValue)
        {
            _targetMap.Remove(key);
            return;
        }
        
        _targetMap[key] = conversion.Serialize(value.Value);
    }
    
    public void SetArray<TItem>(ref List<TItem> value, string key, BymlConversion<TItem> conversion, 
        bool optional = false)
    {
        if (value == null!)
        {
            _targetMap.Remove(key);
            return;
        }
        
        var list = value;
        var bymlArray = new BymlArray(list.Count);

        foreach (var item in list)
            bymlArray.Add(conversion.Serialize(item));

        _targetMap[key] = bymlArray;
    }
    public void SetMap<TItem>(ref Dictionary<string, TItem> value, string key, BymlConversion<TItem> conversion, 
        bool optional = false)
    {
        if (value == null!)
        {
            _targetMap.Remove(key);
            return;
        }
        
        var bymlMap = new BymlMap();
        var map = value;

        foreach ((string mapKey, var item) in map) 
            bymlMap[mapKey] = conversion.Serialize(item);
        
        _targetMap[key] = bymlMap;
    }
}