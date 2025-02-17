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
    protected override void Deserialize(ISerializationContext ctx)
    {
        base.Deserialize(ctx);
        ctx.SetArray(ref WorldMapStagePath, "WorldMapStagePath", STRING);
    }

    protected override void Serialize(ISerializationContext ctx)
    {
        base.Serialize(ctx);
        ctx.SetArray(ref WorldMapStagePath, "WorldMapStagePath", STRING);
    }
    #endregion
}