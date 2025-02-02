using System.Numerics;
using BymlLibrary;
using BymlLibrary.Nodes.Containers;
using Fushigi.Data.LevelObjects;
using static Fushigi.Data.Files.CourseArea;
using static Fushigi.Data.LevelObjects.CourseBgUnit;

namespace Fushigi.Data.BymlSerialization;

public static class SpecialConversions
{
    #region Float3

    public static readonly BymlConversion<Vector3> Float3 = new(ParseFloat3, SerializeFloat3);
    public static Vector3 ParseFloat3(Byml node)
    {
        var array = node.GetArray();
        return new Vector3(
            array[0].GetFloat(),
            array[1].GetFloat(),
            array[2].GetFloat()
            );
    }
    public static Byml SerializeFloat3(Vector3 value)
    {
        return new Byml([
            new Byml(value.X),
            new Byml(value.Y),
            new Byml(value.Z)
        ]);
    }
    #endregion

    #region Vector3
    public static readonly BymlConversion<Vector3> Vector3D = new(ParseVector3D, SerializeVector3D);
    public static Vector3 ParseVector3D(Byml node)
    {
        var map = node.GetMap();
        return new Vector3(
            map["X"].GetFloat(),
            map["Y"].GetFloat(),
            map["Z"].GetFloat()
            );
    }

    public static Byml SerializeVector3D(Vector3 value)
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
    public static readonly BymlConversion<PropertyDict> PropertyDict = new(ParsePropertyDict, SerializePropertyDict);
    public static PropertyDict ParsePropertyDict(Byml node)
    {
        var map = node.GetMap();

        List<PropertyDict.Entry> entries = [];
        foreach (var (key, value) in map)
        {
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
                BymlNodeType.Array => ParseFloat3(value),
                BymlNodeType.Null => null!,
                _ => throw new Exception($"Unexpected node type {value.Type}"),
            };

            entries.Add(new(key, parsed));
        }

        return new PropertyDict(entries);
    }

    public static Byml SerializePropertyDict(PropertyDict dict)
    {
        var map = new BymlMap();

        foreach (var entry in dict)
        {
            Byml serialized = entry.Value switch
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