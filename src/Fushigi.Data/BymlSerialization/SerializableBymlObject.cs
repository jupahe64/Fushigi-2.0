﻿using BymlLibrary;
using BymlLibrary.Nodes.Containers;
using static Fushigi.Data.BymlSerialization.BymlSerialization;
// ReSharper disable RedundantTypeArgumentsOfMethod
// ReSharper disable MemberCanBeMadeStatic.Global

namespace Fushigi.Data.BymlSerialization;

public class MissingBymlKeyException(string key) : Exception($"Couldn't find required key {key} in the byml node");

public abstract class SerializableBymlObject<T> : IDeserializedBymlObject<T>
    where T : SerializableBymlObject<T>, new()
{
    private BymlMap? _map;

    public static T DeserializeFrom(Byml byml)
    {
        var obj = new T
        {
            _map = byml.GetMap()
        };
        obj.Deserialize(new Deserializer(byml.GetMap()));
        return obj;
    }

    protected abstract void Deserialize(Deserializer d);

    public Byml Serialize()
    {
        _map ??= [];

        Serialize(new Serializer(_map));
        return new Byml(_map!);
    }

    public static Byml Serialize(T obj) => obj.Serialize();

    protected abstract void Serialize(Serializer s);

    public class Deserializer(BymlMap map)
    {
        public IReadOnlyDictionary<string, Byml> Map => map;
        
        public void SetBymlValue(ref Byml value, string name)
        {
            if (!map.TryGetValue(name, out var node))
                return;

            value = node;
        }

        public void SetObject<TObject>(ref TObject value, string name)
            where TObject : IDeserializedBymlObject<TObject>
        {
            if (!map.TryGetValue(name, out var node))
                return;

            value = TObject.DeserializeFrom(node);
        }
        
        public void SetValue<TValue>(ref TValue value, string name, BymlConversion<TValue> conversion)
        {
            if (!map.TryGetValue(name, out var node))
                return;

            value = conversion.Deserialize(node);
        }

        #region generated code

        //generated by the code below (with the code in BymlSerialization.cs as input)

        /*
        var regex = new System.Text.RegularExpressions.Regex("\\s*public static (.*) Parse(.*?)\\(Byml byml(.*?)\\).*");

        foreach (string line in input.Split('\n')) {
            var m = regex.Match(line);
            if (!m.Success)
                continue;

            var valueType = m.Groups[1].Value;
            var suffix = m.Groups[2].Value;
            var mapperParameter = m.Groups[3].Value;

            if (string.IsNullOrEmpty(mapperParameter))
            {
                Console.WriteLine($$"""
                    public Func<Byml, {{valueType}}> Convert{{suffix}} => Parse{{suffix}};
            """);
            }
            else
            {
                Console.WriteLine($$"""
                    public void Set{{suffix}}(ref {{valueType}} value, string name, BymlConversion<TItem> conversion, bool optional = false)
                        => Set{{suffix}}(ref value, name, conversion.Deserialize);
            """);
            }
            Console.WriteLine($$"""
                    public void Set{{suffix}}(ref {{valueType}} value, string name{{mapperParameter}}, bool optional = false)
                    {
                        if (!map.TryGetValue(name, out var node)) {
                            if (!optional)
                                throw new MissingBymlKeyException(name);
                            return;
                        }

                        value = Parse{{suffix}}(node{{(mapperParameter == "" ? "" : $", {mapperParameter.Split(' ')[^1]}")}});
                    }
            """);
        }
        */
        public void SetArray<TItem>(ref List<TItem> value, string name, BymlConversion<TItem> conversion, bool optional = false)
            => SetArray<TItem>(ref value, name, conversion.Deserialize);
        public void SetArray<TItem>(ref List<TItem> value, string name, Func<Byml, TItem> mapper, bool optional = false)
        {
            if (!map.TryGetValue(name, out var node)) {
                if (!optional)
                    throw new MissingBymlKeyException(name);
                return;
            }

            value = ParseArray<TItem>(node, mapper);
        }
        public void SetMap<TItem>(ref Dictionary<string, TItem> value, string name, BymlConversion<TItem> conversion, bool optional = false)
            => SetMap<TItem>(ref value, name, conversion.Deserialize);
        public void SetMap<TItem>(ref Dictionary<string, TItem> value, string name, Func<Byml, TItem> mapper, bool optional = false)
        {
            if (!map.TryGetValue(name, out var node)) {
                if (!optional)
                    throw new MissingBymlKeyException(name);
                return;
            }

            value = ParseMap<TItem>(node, mapper);
        }
        public Func<Byml, int> ConvertInt32 => ParseInt32;
        public void SetInt32(ref int value, string name, bool optional = false)
        {
            if (!map.TryGetValue(name, out var node)) {
                if (!optional)
                    throw new MissingBymlKeyException(name);
                return;
            }

            value = ParseInt32(node);
        }
        public Func<Byml, uint> ConvertUInt32 => ParseUInt32;
        public void SetUInt32(ref uint value, string name, bool optional = false)
        {
            if (!map.TryGetValue(name, out var node)) {
                if (!optional)
                    throw new MissingBymlKeyException(name);
                return;
            }

            value = ParseUInt32(node);
        }
        public Func<Byml, long> ConvertInt64 => ParseInt64;
        public void SetInt64(ref long value, string name, bool optional = false)
        {
            if (!map.TryGetValue(name, out var node)) {
                if (!optional)
                    throw new MissingBymlKeyException(name);
                return;
            }

            value = ParseInt64(node);
        }
        public Func<Byml, ulong> ConvertUInt64 => ParseUInt64;
        public void SetUInt64(ref ulong value, string name, bool optional = false)
        {
            if (!map.TryGetValue(name, out var node)) {
                if (!optional)
                    throw new MissingBymlKeyException(name);
                return;
            }

            value = ParseUInt64(node);
        }
        public Func<Byml, float> ConvertFloat => ParseFloat;
        public void SetFloat(ref float value, string name, bool optional = false)
        {
            if (!map.TryGetValue(name, out var node)) {
                if (!optional)
                    throw new MissingBymlKeyException(name);
                return;
            }

            value = ParseFloat(node);
        }
        public Func<Byml, double> ConvertDouble => ParseDouble;
        public void SetDouble(ref double value, string name, bool optional = false)
        {
            if (!map.TryGetValue(name, out var node)) {
                if (!optional)
                    throw new MissingBymlKeyException(name);
                return;
            }

            value = ParseDouble(node);
        }
        public Func<Byml, string> ConvertString => ParseString;
        public void SetString(ref string value, string name, bool optional = false)
        {
            if (!map.TryGetValue(name, out var node)) {
                if (!optional)
                    throw new MissingBymlKeyException(name);
                return;
            }

            value = ParseString(node);
        }
        public Func<Byml, bool> ConvertBool => ParseBool;
        public void SetBool(ref bool value, string name, bool optional = false)
        {
            if (!map.TryGetValue(name, out var node)) {
                if (!optional)
                    throw new MissingBymlKeyException(name);
                return;
            }

            value = ParseBool(node);
        }
        #endregion
    }

    public class Serializer(BymlMap map)
    {
        public BymlMap Map => map;
        
        public void SetBymlValue(ref Byml value, string name)
        {
            if (value == null!)
                return;

            map[name] = value;
        }

        public void SetObject<TObject>(ref TObject value, string name)
            where TObject : class, IDeserializedBymlObject
        {
            if (value == null!)
                return;

            map[name] = value.Serialize();
        }
        
        public void SetValue<TValue>(ref TValue value, string name, BymlConversion<TValue> conversion)
        {
            if (value == null!)
                return;

            map[name] = conversion.Serialize(value);
        }

        #region generated code

        //generated by the following code (with the code in BymlSerialization.cs as input)

        /*
        var regex = new System.Text.RegularExpressions.Regex(
            "\\s*public static Byml Serialize(.*?)\\(([a-zA-Z]*(?:<.*?>)?) [a-zA-Z]*?(,.*?)?\\).*");

        foreach (string line in input.Split('\n')) {
            var m = regex.Match(line);
            if (!m.Success)
                continue;

            var suffix = m.Groups[1].Value;
            var valueType = m.Groups[2].Value;
            var mapperParameter = m.Groups[3].Value;
            
            if (string.IsNullOrEmpty(mapperParameter))
            {
                Console.WriteLine($$"""
                    public Func<{{valueType}}, Byml> Convert{{suffix}} => Serialize{{suffix}};
            """);
            }
            else
            {
                Console.WriteLine($$"""
                    public void Set{{suffix}}(ref {{valueType}} value, string name, BymlConversion<TItem> conversion, bool optional = false)
                        => Set{{suffix}}(ref value, name, conversion.Serialize, optional);
            """);
            }

            Console.WriteLine($$"""
                    public void Set{{suffix}}(ref {{valueType}} value, string name{{mapperParameter}}, bool optional = false)
                    {
                        map[name] = Serialize{{suffix}}(value{{(mapperParameter == "" ? "" : $", {mapperParameter.Split(' ')[^1]}")}});
                    }
            """);
        }
        */
        public void SetArray<TItem>(ref List<TItem> value, string name, BymlConversion<TItem> conversion, bool optional = false)
            => SetArray<TItem>(ref value, name, conversion.Serialize, optional);
        public void SetArray<TItem>(ref List<TItem> value, string name, Func<TItem, Byml> mapper, bool optional = false)
        {
            map[name] = SerializeArray<TItem>(value, mapper);
        }
        public void SetMap<TItem>(ref Dictionary<string, TItem> value, string name, BymlConversion<TItem> conversion, bool optional = false)
            => SetMap<TItem>(ref value, name, conversion.Serialize, optional);
        public void SetMap<TItem>(ref Dictionary<string, TItem> value, string name, Func<TItem, Byml> mapper, bool optional = false)
        {
            map[name] = SerializeMap<TItem>(value, mapper);
        }
        public Func<int, Byml> ConvertInt32 => SerializeInt32;
        public void SetInt32(ref int value, string name, bool optional = false)
        {
            map[name] = SerializeInt32(value);
        }
        public Func<uint, Byml> ConvertUInt32 => SerializeUInt32;
        public void SetUInt32(ref uint value, string name, bool optional = false)
        {
            map[name] = SerializeUInt32(value);
        }
        public Func<long, Byml> ConvertInt64 => SerializeInt64;
        public void SetInt64(ref long value, string name, bool optional = false)
        {
            map[name] = SerializeInt64(value);
        }
        public Func<ulong, Byml> ConvertUInt64 => SerializeUInt64;
        public void SetUInt64(ref ulong value, string name, bool optional = false)
        {
            map[name] = SerializeUInt64(value);
        }
        public Func<float, Byml> ConvertFloat => SerializeFloat;
        public void SetFloat(ref float value, string name, bool optional = false)
        {
            map[name] = SerializeFloat(value);
        }
        public Func<double, Byml> ConvertDouble => SerializeDouble;
        public void SetDouble(ref double value, string name, bool optional = false)
        {
            map[name] = SerializeDouble(value);
        }
        public Func<string, Byml> ConvertString => SerializeString;
        public void SetString(ref string value, string name, bool optional = false)
        {
            map[name] = SerializeString(value);
        }
        public Func<bool, Byml> ConvertBool => SerializeBool;
        public void SetBool(ref bool value, string name, bool optional = false)
        {
            map[name] = SerializeBool(value);
        }
        #endregion
    }
}

public interface IDeserializedBymlObject<T> : IDeserializedBymlObject
{
    public static abstract T DeserializeFrom(Byml byml);
}

public interface IDeserializedBymlObject
{
    public Byml Serialize();
}