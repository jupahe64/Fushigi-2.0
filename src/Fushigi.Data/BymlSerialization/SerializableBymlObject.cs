using System.Numerics;
using BymlLibrary;
using BymlLibrary.Nodes.Containers;
// ReSharper disable RedundantTypeArgumentsOfMethod
// ReSharper disable MemberCanBeMadeStatic.Global

namespace Fushigi.Data.BymlSerialization;

public abstract class SerializableBymlObject<T> : IDeserializedBymlObject<T>
    where T : SerializableBymlObject<T>, new()
{
    public static async Task<(bool success, T value)> DeserializeFrom(Byml byml,
        IBymlDeserializeErrorHandler errorHandler)
    {
        return await Deserializer.Deserialize(byml, DeserializeFunc, errorHandler);
    }
    
    public static readonly BymlConversion<T> Conversion = new(BymlNodeType.Map, DeserializeFunc, SerializeFunc);

    private static T DeserializeFunc(Deserializer deserializer)
    {
        var obj = new T
        {
            _map = deserializer.GetNode().GetMap()
        };
        obj.Deserialize(deserializer);
        return obj;
    }
    
    private static Byml SerializeFunc(T obj, Serializer serializer)
    {
        obj._map ??= [];
        if (!serializer.TryGetBaseNode(BymlNodeType.Map, out _))
            serializer.SetBaseNode(obj._map);
        obj.Serialize(serializer);
        return new Byml(obj._map!);
    }

    public Byml Serialize()
    {
        _map ??= [];

        Serialize(new Serializer(_map));
        return new Byml(_map!);
    }
    
    // ReSharper disable InconsistentNaming
    protected readonly BymlConversion<int>    INT32 =  Conversions.Int32;
    protected readonly BymlConversion<uint>   UINT32 = Conversions.UInt32;
    protected readonly BymlConversion<long>   INT64 =  Conversions.Int64;
    protected readonly BymlConversion<ulong>  UINT64 = Conversions.UInt64;
    protected readonly BymlConversion<float>  FLOAT =  Conversions.Float;
    protected readonly BymlConversion<double> DOUBLE = Conversions.Double;
    protected readonly BymlConversion<string> STRING = Conversions.String;
    protected readonly BymlConversion<bool>   BOOL   = Conversions.Bool;
    
    protected readonly BymlConversion<Vector3>      FLOAT3        = SpecialConversions.Float3;
    protected readonly BymlConversion<Vector3>      VECTOR3D      = SpecialConversions.Vector3D;
    protected readonly BymlConversion<PropertyDict> PROPERTY_DICT = SpecialConversions.PropertyDict;
    // ReSharper restore InconsistentNaming
    
    protected abstract void Deserialize(ISerializationContext ctx);
    protected abstract void Serialize(ISerializationContext ctx);
    
    private BymlMap? _map;
}

public interface IDeserializedBymlObject<T> : IDeserializedBymlObject
{
    public static abstract Task<(bool success, T value)> DeserializeFrom(Byml byml,
        IBymlDeserializeErrorHandler errorHandler);
}

public interface IDeserializedBymlObject
{
    public Byml Serialize();
}