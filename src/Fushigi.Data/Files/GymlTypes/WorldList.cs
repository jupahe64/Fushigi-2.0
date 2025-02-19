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

    protected override void Serialization<TContext>(TContext ctx)
    {
        base.Serialization(ctx);
        ctx.SetArray(ref WorldMapStagePath, "WorldMapStagePath", STRING);
    }
    #endregion
}