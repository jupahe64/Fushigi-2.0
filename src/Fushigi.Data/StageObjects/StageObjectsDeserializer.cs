using System.Collections;
using System.Runtime.InteropServices;
using BymlLibrary;
using BymlLibrary.Nodes.Containers;
using Fushigi.Data.BymlSerialization;

namespace Fushigi.Data.StageObjects;

public struct ListSegment<T>
{
    internal readonly int Start;
    internal readonly int End;
    public int Count => End-Start;

    public ListSegment(int start, int end)
    {
        Start = start;
        End = end;
    }
}

public struct ListRef<T>
{
    internal readonly int Index;

    internal ListRef(int index)
    {
        Index = index;
    }
}

public interface IDeserializedStageDataKeeper
{
    public readonly struct ListSegmentData<T>(ListSegment<T> segment, List<T> list)
    {
        public static readonly ListSegmentData<T> Empty = new();
        private readonly ListSegment<T> _segment = segment;
        private readonly List<T> _list = list;
        public Enumerator GetEnumerator() => new Enumerator(this);

        public struct Enumerator(ListSegmentData<T> @ref)
        {
            private int _index = @ref._segment.Start - 1;

            public bool MoveNext()
            {
                _index++;
                return _index < @ref._segment.End;
            }
            
            public T Current => @ref._list[_index];
        }
    }
    public ListSegmentData<T> GetData<T>(ListSegment<T> segment);
    public T GetData<T>(ListRef<T> listRef);
    public ListSegmentData<T> GetData<T>(ListSegment<T>? segment)
    {
        return segment.HasValue ? GetData<T>(segment.Value) : ListSegmentData<T>.Empty;
    }
}

internal class StageObjectsDeserializer : IDeserializedStageDataKeeper
{
    public IDeserializedStageDataKeeper.ListSegmentData<T> GetData<T>(ListSegment<T> segment)
    {
        if (!_stageObjectLists.TryGetValue(typeof(T), out var listObject))
            throw new ArgumentException("No objects of given type", nameof(T));
        
        var list = (List<T>)listObject;
        
        int start = segment.Start;
        int end = segment.End;
        if (!(0 <= start && start <= end && end <= list.Count))
            throw new ArgumentException("Invalid range", nameof(segment));
        
        return new IDeserializedStageDataKeeper.ListSegmentData<T>(segment, list);
    }
    
    public T GetData<T>(ListRef<T> listRef)
    {
        if (!_stageObjectLists.TryGetValue(typeof(T), out var listObject))
            throw new ArgumentException("No objects of given type", nameof(T));
        
        var list = (List<T>)listObject;
        
        int idx = listRef.Index;
        if (!(0 <= idx && idx <= list.Count))
            throw new ArgumentException("Invalid index", nameof(listRef));
        
        return CollectionsMarshal.AsSpan(list)[listRef.Index];
    }
    
    internal delegate T DeserializeFunc<out T>(Deserializer d, StageObjectsDeserializer sod);
    
    internal ListRef<T>? Deserialize<T>(Deserializer deserializer, string key, 
        DeserializeFunc<T> deserialize, BymlNodeType? requiredNodeType = BymlNodeType.Map, bool optional = false)
    {
        var bymlMap = deserializer.GetNode().GetMap();
        if (!bymlMap.TryGetValue(key, out var value))
        {
            if (optional)
                return null;
            
            deserializer.ReportMissingKey(key);
            return new ListRef<T>(-1);
        }
        
        if (requiredNodeType.HasValue && value.Type != requiredNodeType.Value)
        {
            deserializer.ReportUnexpectedType(requiredNodeType);
            return new ListRef<T>(-1);
        }
        
        var valueDeserializer = deserializer.CreateDeserializerFor(value);
        return Deserialize(valueDeserializer, deserialize);
    }
    
    internal ListSegment<T>? DeserializeArray<T>(Deserializer deserializer, string key, 
        DeserializeFunc<T> deserialize, BymlNodeType? requiredNodeType = BymlNodeType.Map, bool optional = false)
    {
        var bymlMap = deserializer.GetNode().GetMap();
        if (!bymlMap.TryGetValue(key, out var value))
        {
            if (optional)
                return null;
            
            deserializer.ReportMissingKey(key);
            return new ListSegment<T>(-1, -1);
        }

        var valueDeserializer = deserializer.CreateDeserializerFor(value);
        return DeserializeArray(valueDeserializer, deserialize, requiredNodeType);
    }
    
    private ListRef<T> Deserialize<T>(Deserializer deserializer, DeserializeFunc<T> deserialize)
    {
        var list = GetOrAddList<T>();
        int idx = list.Count;
        list.Add(deserialize(deserializer, this));
        return new ListRef<T>(idx);
    }

    private ListSegment<T> DeserializeArray<T>(Deserializer deserializer, DeserializeFunc<T> deserialize, 
        BymlNodeType? requiredNodeType = BymlNodeType.Map)
    {
        var bymlNode = deserializer.GetNode();
        if (bymlNode.Value is not BymlArray bymlArray)
            throw new InvalidDataException("node is not a BymlArray");

        var list = GetOrAddList<T>();
        int start = list.Count, end = list.Count;
        foreach (var item in bymlArray)
        {
            var itemDeserializer = deserializer.CreateDeserializerFor(item);
            if (requiredNodeType.HasValue && item.Type != requiredNodeType.Value)
            {
                itemDeserializer.ReportUnexpectedType(requiredNodeType);
                list.Add(default!);
            }
            else
                list.Add(deserialize(itemDeserializer, this));

            end++;
        }
        return new ListSegment<T>(start, end);
    }

    private List<T> GetOrAddList<T>()
    {
        List<T> list;
        if (_stageObjectLists.TryGetValue(typeof(T), out var listObject))
            list = (List<T>)listObject;
        else
        {
            list = [];
            _stageObjectLists.Add(typeof(T), list);
        }
        return list;
    }
    
    private readonly Dictionary<Type, IList> _stageObjectLists = [];
}