using System.Numerics;
using Fushigi.Data.BymlSerialization;

namespace Fushigi.Data.StageObjects;

public class StageRail : SerializableBymlObject<StageRail>
{
    public List<RailPoint> Points = null!;
    public uint AreaHash;
    public ulong Hash;
    public string Gyaml = null!;
    public bool IsClosed;
    public PropertyDict Dynamic = null!;

    protected override void Deserialize(Deserializer d)
    {
        d.SetArray(ref Points, "Points", RailPoint.Conversion);
        d.SetUInt32(ref AreaHash, "AreaHash");
        d.SetUInt64(ref Hash, "Hash");
        d.SetString(ref Gyaml, "Gyaml");
        d.SetBool(ref IsClosed, "IsClosed");
        d.SetValue(ref Dynamic, "Dynamic", SpecialConversions.PropertyDict);
    }

    protected override void Serialize(Serializer s)
    {
        s.SetArray(ref Points, "Points", RailPoint.Conversion);
        s.SetUInt32(ref AreaHash, "AreaHash");
        s.SetUInt64(ref Hash, "Hash");
        s.SetString(ref Gyaml, "Gyaml");
        s.SetBool(ref IsClosed, "IsClosed");
        s.SetValue(ref Dynamic, "Dynamic", SpecialConversions.PropertyDict);
    }
}
public class RailPoint : SerializableBymlObject<RailPoint>
{
    public ulong Hash;
    public PropertyDict Dynamic = null!;
    public Vector3 Translate;
    public Vector3 CurveControl;

    protected override void Deserialize(Deserializer d)
    {
        d.SetUInt64(ref Hash, "Hash", true);
        d.SetValue(ref Dynamic, "Dynamic", SpecialConversions.PropertyDict);
        d.SetValue(ref Translate, "Translate", SpecialConversions.Float3);
        d.SetValue(ref CurveControl, "Control1", SpecialConversions.Float3);
    }

    protected override void Serialize(Serializer s)
    {
        s.SetUInt64(ref Hash, "Hash", true);
        s.SetValue(ref Dynamic, "Dynamic", SpecialConversions.PropertyDict);
        s.SetValue(ref Translate, "Translate", SpecialConversions.Float3);
        s.SetValue(ref CurveControl, "Control1", SpecialConversions.Float3);
    }
}