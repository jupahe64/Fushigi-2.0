using Fushigi.Data.BymlSerialization;

namespace Fushigi.Data.Files.GymlTypes;

public class StageParam : SerializableBymlObject<StageParam>
{
    public static readonly string GymlTypeSuffix = "game__stage__StageParam";
    public static readonly string[] DefaultSavePath = ["Stage", "StageParam"];

    public PropertyDict Components = null!;
    protected override void Deserialize(Deserializer d)
    {
        d.SetValue(ref Components, "Components", SpecialConversions.PropertyDict);
    }

    protected override void Serialize(Serializer s)
    {
        s.SetValue(ref Components, "Components", SpecialConversions.PropertyDict);
    }
}