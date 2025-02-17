using BymlLibrary;
using TYPE = BymlLibrary.BymlNodeType;

namespace Fushigi.Data.BymlSerialization;

public record struct BymlConversion<TValue>(BymlNodeType? RequiredNodeType, 
    Func<Deserializer, TValue> Deserialize, Func<TValue, Serializer, Byml> Serialize);

public class Conversions
{
    public static readonly BymlConversion<int>    Int32 =  new(TYPE.Int,    DeserializeValue<int>,    (v, _) => new Byml(v));
    public static readonly BymlConversion<uint>   UInt32 = new(TYPE.UInt32, DeserializeValue<uint>,   (v, _) => new Byml(v));
    public static readonly BymlConversion<long>   Int64 =  new(TYPE.Int64,  DeserializeValue<long>,   (v, _) => new Byml(v));
    public static readonly BymlConversion<ulong>  UInt64 = new(TYPE.UInt64, DeserializeValue<ulong>,  (v, _) => new Byml(v));
    public static readonly BymlConversion<float>  Float =  new(TYPE.Float,  DeserializeValue<float>,  (v, _) => new Byml(v));
    public static readonly BymlConversion<double> Double = new(TYPE.Double, DeserializeValue<double>, (v, _) => new Byml(v));
    public static readonly BymlConversion<string> String = new(TYPE.String, DeserializeValue<string>, (v, _) => new Byml(v));
    public static readonly BymlConversion<bool>   Bool =   new(TYPE.Bool,   DeserializeValue<bool>,   (v, _) => new Byml(v));
    
    private static TValue DeserializeValue<TValue>(Deserializer deserializer)
    {
        return deserializer.GetNode().Get<TValue>();
    }
}