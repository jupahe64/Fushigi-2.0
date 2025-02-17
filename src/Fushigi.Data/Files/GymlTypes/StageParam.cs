using Fushigi.Data.BymlSerialization;

namespace Fushigi.Data.Files.GymlTypes;

public class StageParam : GymlFile<StageParam>
{
    public static readonly string GymlTypeSuffix = "game__stage__StageParam";
    public static readonly string[] DefaultSavePath = ["Stage", "StageParam"];

    public StageComponents Components = null!;
    protected override void Deserialize(ISerializationContext ctx)
    {
        base.Deserialize(ctx);
        ctx.SetObject(ref Components, "Components");
    }

    protected override void Serialize(ISerializationContext ctx)
    {
        base.Serialize(ctx);
        ctx.SetObject(ref Components, "Components");
    }

    public class StageComponents : SerializableBymlObject<StageComponents>
    {
        public string? AreaParam;
        public string? BakeSource;
        public string? CourseInfo;
        public string? MapAnalysisInfo;
        public string? Mumap;
        public string? StageLoadInfo;
        public string? StageSequenceInfo;
        public string? StaticCompoundBodySourceParam;
        public string? WorldList;
        public string? WorldMapAnalysisInfo;
        public string? WorldMapInfo;
        
        protected override void Deserialize(ISerializationContext ctx)
        {
            ctx.Set(STRING, ref AreaParam!, "AreaParam", optional: true);
            ctx.Set(STRING, ref BakeSource!, "BakeSource", optional: true);
            ctx.Set(STRING, ref CourseInfo!, "CourseInfo", optional: true);
            ctx.Set(STRING, ref MapAnalysisInfo!, "MapAnalysisInfo", optional: true);
            ctx.Set(STRING, ref Mumap!, "Mumap", optional: true);
            ctx.Set(STRING, ref StageLoadInfo!, "StageLoadInfo", optional: true);
            ctx.Set(STRING, ref StageSequenceInfo!, "StageSequenceInfo", optional: true);
            ctx.Set(STRING, ref StaticCompoundBodySourceParam!, "StaticCompoundBodySourceParam", optional: true);
            ctx.Set(STRING, ref WorldList!, "WorldList", optional: true);
            ctx.Set(STRING, ref WorldMapAnalysisInfo!, "WorldMapAnalysisInfo", optional: true);
            ctx.Set(STRING, ref WorldMapInfo!, "WorldMapInfo", optional: true);
        }

        protected override void Serialize(ISerializationContext ctx)
        {
            ctx.Set(STRING, ref AreaParam!, "AreaParam", optional: true);
            ctx.Set(STRING, ref BakeSource!, "BakeSource", optional: true);
            ctx.Set(STRING, ref CourseInfo!, "CourseInfo", optional: true);
            ctx.Set(STRING, ref MapAnalysisInfo!, "MapAnalysisInfo", optional: true);
            ctx.Set(STRING, ref Mumap!, "Mumap", optional: true);
            ctx.Set(STRING, ref StageLoadInfo!, "StageLoadInfo", optional: true);
            ctx.Set(STRING, ref StageSequenceInfo!, "StageSequenceInfo", optional: true);
            ctx.Set(STRING, ref StaticCompoundBodySourceParam!, "StaticCompoundBodySourceParam", optional: true);
            ctx.Set(STRING, ref WorldList!, "WorldList", optional: true);
            ctx.Set(STRING, ref WorldMapAnalysisInfo!, "WorldMapAnalysisInfo", optional: true);
            ctx.Set(STRING, ref WorldMapInfo!, "WorldMapInfo", optional: true);
        }
    }
}