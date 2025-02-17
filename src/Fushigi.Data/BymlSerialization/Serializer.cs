using System.Diagnostics.CodeAnalysis;
using BymlLibrary;
using BymlLibrary.Nodes.Containers;

namespace Fushigi.Data.BymlSerialization;

public class Serializer : ISerializationContext
{
    public Serializer(Byml baseNode)
    {
        _baseNode = baseNode;
    }

    private Byml? _baseNode;

    public void SetBaseNode(Byml node)
    {
        _baseNode = node;
    }

    public bool TryGetBaseNode(BymlNodeType? requiredNodeType, [NotNullWhen(true)] out Byml? node)
    {
        node = null;
        if (_baseNode == null)
            return false;
        
        if (requiredNodeType.HasValue && _baseNode.Type != requiredNodeType)
            return false;
        
        node = _baseNode;
        return true;
    }

    private static Byml? GetExistingNode(BymlMap map, string key, BymlNodeType? requiredNodeType)
    {
        Byml? existingNode = null;
        if (map.TryGetValue(key, out var node) &&
            (!requiredNodeType.HasValue || node.Type == requiredNodeType)
           )
        {
            existingNode = node;
        }
        return existingNode;
    }
    
    private static Byml? GetExistingNode(BymlArray array, int index, BymlNodeType? requiredNodeType)
    {
        Byml? existingNode = null;
        if (array.Count > index &&
            (!requiredNodeType.HasValue || array[index].Type == requiredNodeType)
           )
        {
            existingNode = array[index];
        }
        return existingNode;
    }
    
    public void Set<TValue>(BymlConversion<TValue> conversion, ref TValue value, string key, 
        bool optional = false)
    {
        if (value == null)
            return;
        
        var targetMap = _baseNode!.GetMap();
        var existingNode = GetExistingNode(targetMap, key, conversion.RequiredNodeType);
        var before = _baseNode;
        _baseNode = existingNode;
        targetMap[key] = conversion.Serialize(value, this);
        _baseNode = before; 
    }
    public void Set<TValue>(BymlConversion<TValue> conversion, ref TValue? value, string key, 
        bool optional = false)
        where TValue : struct
    {
        if (!value.HasValue)
            return;
        
        var targetMap = _baseNode!.GetMap();
        var existingNode = GetExistingNode(targetMap, key, conversion.RequiredNodeType);
        var before = _baseNode;
        _baseNode = existingNode;
        targetMap[key] = conversion.Serialize(value.Value, this);
        _baseNode = before; 
    }
    
    public void SetArray<TItem>(ref List<TItem> value, string key, BymlConversion<TItem> conversion, 
        bool optional = false)
    {
        var targetMap = _baseNode!.GetMap();
        
        //this should not throw
        var bymlArray = GetExistingNode(targetMap, key, BymlNodeType.Array)?.GetArray() ?? [];
        var list = value;

        for (var idx = 0; idx < list.Count; idx++)
        {
            var item = list[idx];
            var before = _baseNode;
            _baseNode = GetExistingNode(bymlArray, idx, conversion.RequiredNodeType);
            if (idx < bymlArray.Count)
                bymlArray[idx] = conversion.Serialize(item, this);
            else
                bymlArray.Add(conversion.Serialize(item, this));
            _baseNode = before;
        }

        targetMap[key] = bymlArray;
    }
    public void SetMap<TItem>(ref Dictionary<string, TItem> value, string key, BymlConversion<TItem> conversion, 
        bool optional = false)
    {
        var targetMap = _baseNode!.GetMap();
        
        //this should not throw
        var bymlMap = GetExistingNode(targetMap, key, BymlNodeType.Map)?.GetMap() ?? [];
        var map = value;

        foreach ((string mapKey, var item) in map)
        {
            var before = _baseNode;
            _baseNode = GetExistingNode(bymlMap, mapKey, conversion.RequiredNodeType);
            bymlMap[mapKey] = conversion.Serialize(item, this);
            _baseNode = before; 
        }

        value = map;
    }
}