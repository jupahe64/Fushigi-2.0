using Fushigi.Data.BymlSerialization;

namespace Fushigi.Data.Files.GymlTypes;

/// <summary>
/// Serializable game.stage.WorldList
/// </summary>
public class WorldList : GymlFile<WorldList>
{
    public static readonly string GymlTypeSuffix = "game__stage__WorldList";
    public static readonly string[] DefaultSavePath = ["Stage", "WorldList"];
    
    public List<string> WorldMapStagePath = null!;
    
    #region De/Serialization
    protected override void Deserialize(Deserializer d)
    {
        base.Deserialize(d);
        d.SetArray(ref WorldMapStagePath, "WorldMapStagePath", d.ConvertString);
    }

    protected override void Serialize(Serializer s)
    {
        base.Serialize(s);
        s.SetArray(ref WorldMapStagePath, "WorldMapStagePath", s.ConvertString);
    }
    #endregion
}