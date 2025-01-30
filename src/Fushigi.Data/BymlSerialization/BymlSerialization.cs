using System.Numerics;
using BymlLibrary;
using BymlLibrary.Nodes.Containers;

namespace Fushigi.Data.BymlSerialization;

public record struct BymlConversion<TValue>(Func<Byml, TValue> Deserialize, Func<TValue, Byml> Serialize);
public static class BymlSerialization
{
    public static List<TItem> ParseArray<TItem>(Byml byml, Func<Byml, TItem> mapper)
    {
        var bymlArray = byml.GetArray();
        var list = new List<TItem>();

        foreach (var item in bymlArray)
        {
            list.Add(mapper(item));
        }

        return list;
    }

    public static Dictionary<string, TItem> ParseMap<TItem>(Byml byml, Func<Byml, TItem> mapper)
    {
        var bymlMap = byml.GetMap();
        var dict = new Dictionary<string, TItem>();

        foreach (var (key, value) in bymlMap)
        {
            dict.Add(key, mapper(value));
        }

        return dict;
    }

    public static int ParseInt32(Byml byml) => byml.GetInt();
    public static uint ParseUInt32(Byml byml) => byml.GetUInt32();
    public static long ParseInt64(Byml byml) => byml.GetInt64();
    public static ulong ParseUInt64(Byml byml) => byml.GetUInt64();
    public static float ParseFloat(Byml byml) => byml.GetFloat();
    public static double ParseDouble(Byml byml) => byml.GetDouble();
    public static string ParseString(Byml byml) => byml.GetString();
    public static bool ParseBool(Byml byml) => byml.GetBool();
    
    public static Vector3 ParseVector3D(Byml node)
    {
        var map = node.GetMap();
        return new Vector3(
            map["X"].GetFloat(),
            map["Y"].GetFloat(),
            map["Z"].GetFloat()
        );
    }
    
    public static Byml SerializeArray<TItem>(List<TItem> list, Func<TItem, Byml> mapper)
    {
        var bymlArray = new BymlArray();

        foreach (var item in list)
        {
            bymlArray.Add(mapper(item));
        }

        return new Byml(bymlArray);
    }

    public static Byml SerializeMap<TItem>(Dictionary<string, TItem> dict, Func<TItem, Byml> mapper)
    {
        var bymlMap = new BymlMap();

        foreach (var (key, value) in dict)
        {
            bymlMap.Add(key, mapper(value));
        }

        return new Byml(bymlMap);
    }

    public static Byml SerializeInt32(int value) => new Byml(value);
    public static Byml SerializeUInt32(uint value) => new Byml(value);
    public static Byml SerializeInt64(long value) => new Byml(value);
    public static Byml SerializeUInt64(ulong value) => new Byml(value);
    public static Byml SerializeFloat(float value) => new Byml(value);
    public static Byml SerializeDouble(double value) => new Byml(value);
    public static Byml SerializeString(string value) => new Byml(value);
    public static Byml SerializeBool(bool value) => new Byml(value);
    
    public static Byml SerializeVector3D(Vector3 value)
    {
        return new Byml(new BymlMap
        {
            ["X"] = new Byml(value.X),
            ["Y"] = new Byml(value.Y),
            ["Z"] = new Byml(value.Z)
        });
    }
}