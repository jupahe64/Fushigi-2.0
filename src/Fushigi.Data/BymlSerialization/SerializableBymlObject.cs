using System.Numerics;
using BymlLibrary;
using BymlLibrary.Nodes.Containers;
using Fushigi.Data.Files;
using Fushigi.Data.Files.GymlTypes;

// ReSharper disable RedundantTypeArgumentsOfMethod
// ReSharper disable MemberCanBeMadeStatic.Global

namespace Fushigi.Data.BymlSerialization;

public abstract class SerializableBymlObject<T> : IDeserializedBymlObject<T>
    where T : SerializableBymlObject<T>, new()
{
    public static async Task<(bool success, T value)> DeserializeFrom(Byml byml,
        IBymlDeserializeErrorHandler errorHandler, RomFS.RetrievedFileLocation fileLocationInfo)
    {
        return await Deserializer.Deserialize(byml, DeserializeFunc, errorHandler, fileLocationInfo);
    }
    
    public static readonly BymlConversion<T> Conversion = new(BymlNodeType.Map, DeserializeFunc, SerializeFunc);

    private static T DeserializeFunc(Deserializer deserializer)
    {
        var obj = new T
        {
            _map = deserializer.GetNode().GetMap()
        };
        obj.Serialization(deserializer);
        return obj;
    }
    
    private static Byml SerializeFunc(T obj)
    {
        obj._map ??= [];
        obj.Serialization(new Serializer(obj._map));
        return new Byml(obj._map!);
    }

    public Byml Serialize()
    {
        _map ??= [];

        Serialization(new Serializer(_map));
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
    
    protected BymlConversion<GymlRef<TGymlFile>> GYML_REF<TGymlFile>()
        where TGymlFile : GymlFile<TGymlFile>, IGymlType, new()
        => FileRefConversion.For<GymlRef<TGymlFile>>();
    
    protected readonly BymlConversion<MuMapRef> MU_MAP_REF = FileRefConversion.For<MuMapRef>();
    // ReSharper restore InconsistentNaming

    protected abstract void Serialization<TContext>(TContext ctx) where TContext : struct, ISerializationContext;
    
    private BymlMap? _map;
}

public static class BymlObjectConversions
{
    // ReSharper disable InconsistentNaming
    internal static readonly BymlConversion<int>    INT32 =  Conversions.Int32;
    internal static readonly BymlConversion<uint>   UINT32 = Conversions.UInt32;
    internal static readonly BymlConversion<long>   INT64 =  Conversions.Int64;
    internal static readonly BymlConversion<ulong>  UINT64 = Conversions.UInt64;
    internal static readonly BymlConversion<float>  FLOAT =  Conversions.Float;
    internal static readonly BymlConversion<double> DOUBLE = Conversions.Double;
    internal static readonly BymlConversion<string> STRING = Conversions.String;
    internal static readonly BymlConversion<bool>   BOOL   = Conversions.Bool;
    
    internal static readonly BymlConversion<Vector3>      FLOAT3        = SpecialConversions.Float3;
    internal static readonly BymlConversion<Vector3>      VECTOR3D      = SpecialConversions.Vector3D;
    internal static readonly BymlConversion<PropertyDict> PROPERTY_DICT = SpecialConversions.PropertyDict;
    
    internal static BymlConversion<GymlRef<TGymlFile>> GYML_REF<TGymlFile>()
        where TGymlFile : GymlFile<TGymlFile>, IGymlType, new()
        => FileRefConversion.For<GymlRef<TGymlFile>>();
    
    internal static readonly BymlConversion<MuMapRef> MU_MAP_REF = FileRefConversion.For<MuMapRef>();
    // ReSharper restore InconsistentNaming
}

public interface IDeserializedBymlObject<T> : IDeserializedBymlObject
{
    public static abstract Task<(bool success, T value)> DeserializeFrom(Byml byml,
        IBymlDeserializeErrorHandler errorHandler, RomFS.RetrievedFileLocation fileLocationInfo);
}

public interface IDeserializedBymlObject
{
    public Byml Serialize();
}