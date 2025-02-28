using System.Numerics;
using System.Runtime.CompilerServices;
using BymlLibrary;
using BymlLibrary.Nodes.Containers;

namespace Fushigi.Data.BymlSerialization;

public static class SpecialConversions
{
    #region Enum
    public static BymlConversion<T> GetEnumConversion<T>() 
        where T : struct, Enum
    {
        if (Unsafe.SizeOf<T>() != sizeof(int))
            throw new ArgumentException($"The underlying type of enum {nameof(T)} is not a 32 bit integer.", nameof(T));
        
        return new BymlConversion<T>(BymlNodeType.Int, DeserializeEnum<T>, SerializeEnum);
    }

    private static T DeserializeEnum<T>(Deserializer deserializer)
        where T : struct, Enum
    {
        int value = deserializer.GetNode().GetInt();
        var enumValue = Unsafe.As<int, T>(ref value);
        if (!Enum.IsDefined(enumValue))
        {
            deserializer.ReportUnexpectedEnumValue();
            return default;
        }
        return enumValue;
    }
    
    private static Byml SerializeEnum<T>(T value)
        where T : struct, Enum
    {
        return new Byml(Unsafe.As<T, int>(ref value));
    }
    #endregion
    
    #region Float3

    public static readonly BymlConversion<Vector3> Float3 = 
        new(BymlNodeType.Array, DeserializeFloat3, SerializeFloat3);
    
    private static Vector3 DeserializeFloat3(Deserializer deserializer)
    {
        deserializer.ReportAllMissingKeysAndIndices = true; //Vector3 cannot be partially assigned
        Vector3 vec = default;
        deserializer.Set(Conversions.Float, ref vec.X, 0);
        deserializer.Set(Conversions.Float, ref vec.Y, 1);
        deserializer.Set(Conversions.Float, ref vec.Z, 2);
        return vec;
    }
    private static Byml SerializeFloat3(Vector3 value, Serializer serializer)
    {
        return new Byml([
            new Byml(value.X),
            new Byml(value.Y),
            new Byml(value.Z)
        ]);
    }
    // for PropertyDict
    private static Byml SerializeFloat3(Vector3 value)
    {
        return new Byml([
            new Byml(value.X),
            new Byml(value.Y),
            new Byml(value.Z)
        ]);
    }
    #endregion

    #region Vector3
    public static readonly BymlConversion<Vector3> Vector3D = 
        new(BymlNodeType.Map, DeserializeVector3D, SerializeVector3D);
    
    private static Vector3 DeserializeVector3D(Deserializer deserializer)
    {
        deserializer.ReportAllMissingKeysAndIndices = true; //Vector3 cannot be partially assigned
        Vector3 vec = default;
        deserializer.Set(Conversions.Float, ref vec.X, "X");
        deserializer.Set(Conversions.Float, ref vec.Y, "Y");
        deserializer.Set(Conversions.Float, ref vec.Z, "Z");
        return vec;
    }

    private static Byml SerializeVector3D(Vector3 value)
    {
        return new Byml(new BymlMap
        {
            ["X"] = new Byml(value.X),
            ["Y"] = new Byml(value.Y),
            ["Z"] = new Byml(value.Z)
        });
    }
    #endregion

    #region PropertyDict
    public static readonly BymlConversion<PropertyDict> PropertyDict = 
        new(BymlNodeType.Map, DeserializePropertyDict, SerializePropertyDict);

    private static PropertyDict DeserializePropertyDict(Deserializer deserializer)
    {
        var map = deserializer.GetNode().GetMap();

        List<PropertyDict.Entry> entries = [];
        foreach (var (key, value) in map)
        {
            static Vector3 ParseFloat3(Deserializer deserializer, Byml node) 
                => DeserializeFloat3(deserializer.CreateDeserializerFor(node));

            static object HandleUnexpected(Deserializer deserializer, Byml node)
            {
                deserializer.CreateDeserializerFor(node).ReportUnexpectedType();
                return null!;
            }
            
            object parsed = value.Type switch
            {
                BymlNodeType.String => value.GetString(),
                BymlNodeType.Bool => value.GetBool(),
                BymlNodeType.Int => value.GetInt(),
                BymlNodeType.UInt32 => value.GetUInt32(),
                BymlNodeType.Int64 => value.GetInt64(),
                BymlNodeType.UInt64 => value.GetUInt64(),
                BymlNodeType.Float => value.GetFloat(),
                BymlNodeType.Double => value.GetDouble(),
                BymlNodeType.Array => ParseFloat3(deserializer, value),
                BymlNodeType.Null => null!,
                _ => HandleUnexpected(deserializer, value),
            };

            entries.Add(new(key, parsed));
        }

        return new PropertyDict(entries);
    }

    private static Byml SerializePropertyDict(PropertyDict dict)
    {
        var map = new BymlMap();

        foreach (var entry in dict)
        {
            var serialized = entry.Value switch
            {
                string v => new Byml(v),
                bool v => new Byml(v),
                int v => new Byml(v),
                uint v => new Byml(v),
                long v => new Byml(v),
                ulong v => new Byml(v),
                float v => new Byml(v),
                double v => new Byml(v),
                Vector3 v => SerializeFloat3(v),
                null => new Byml(),
                _ => throw new Exception($"Unexpected object type {entry.Value.GetType()}"),
            };

            map.Add(entry.Key, serialized);
        }

        return new Byml(map);
    }

    #endregion
}