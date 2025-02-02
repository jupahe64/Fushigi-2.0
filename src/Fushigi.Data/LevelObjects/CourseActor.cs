using Fushigi.Data.BymlSerialization;
using Fushigi.Data;
using BymlLibrary;
using System.Numerics;

namespace Fushigi.Data.LevelObjects;

public class CourseActor : SerializableBymlObject<CourseActor>
{
    public uint AreaHash;
    public ulong Hash;
    public string Gyaml = null!;
    public string Name = null!;
    public string Layer = null!;
    public PropertyDict Dynamic = null!;
    public Vector3 Rotate;
    public Vector3 Scale;
    public Vector3 Translate;

    protected override void Deserialize(Deserializer d)
    {
        d.SetUInt32(ref AreaHash, "AreaHash");
        d.SetUInt64(ref Hash, "Hash");
        d.SetString(ref Gyaml, "Gyaml");
        d.SetString(ref Name, "Name");
        d.SetString(ref Layer, "Layer");
        d.SetValue(ref Dynamic, "Dynamic", SpecialConversions.PropertyDict);
        d.SetValue(ref Rotate, "Rotate", SpecialConversions.Float3);
        d.SetValue(ref Scale, "Scale", SpecialConversions.Float3);
        d.SetValue(ref Translate, "Translate", SpecialConversions.Float3);
    }

    protected override void Serialize(Serializer s)
    {
        s.SetUInt32(ref AreaHash, "AreaHash");
        s.SetUInt64(ref Hash, "Hash");
        s.SetString(ref Gyaml, "Gyaml");
        s.SetString(ref Name, "Name");
        s.SetString(ref Layer, "Layer");
        s.SetValue(ref Dynamic, "Dynamic", SpecialConversions.PropertyDict);
        s.SetValue(ref Rotate, "Rotate", SpecialConversions.Float3);
        s.SetValue(ref Scale, "Scale", SpecialConversions.Float3);
        s.SetValue(ref Translate, "Translate", SpecialConversions.Float3);
    }
}
