using Fushigi.Data.BymlSerialization;

namespace Fushigi.Data.Files.GymlTypes;

public class StageParam : GymlFile<StageParam>
{
    public static readonly string GymlTypeSuffix = "game__stage__StageParam";
    public static readonly string[] DefaultSavePath = ["Stage", "StageParam"];

    public StageComponents Components = null!;

    protected override void Serialization<TContext>(TContext ctx)
    {
        base.Serialization(ctx);
        ctx.SetObject(ref Components, "Components");
    }

    public class StageComponents : SerializableBymlObject<StageComponents>
    {
        public GymlRef? AreaParam;
        public GymlRef? BakeSource;
        public GymlRef? CourseInfo;
        public GymlRef? MapAnalysisInfo;
        public MuMapRef? Mumap;
        public GymlRef? StageLoadInfo;
        public GymlRef? StageSequenceInfo;
        public GymlRef? StaticCompoundBodySourceParam;
        public GymlRef? WorldList;
        public GymlRef? WorldMapAnalysisInfo;
        public GymlRef? WorldMapInfo;

        protected override void Serialization<TContext>(TContext ctx)
        {
            ctx.Set(GYML_REF, ref AreaParam!, "AreaParam", optional: true);
            ctx.Set(GYML_REF, ref BakeSource!, "BakeSource", optional: true);
            ctx.Set(GYML_REF, ref CourseInfo!, "CourseInfo", optional: true);
            ctx.Set(GYML_REF, ref MapAnalysisInfo!, "MapAnalysisInfo", optional: true);
            ctx.Set(MU_MAP_REF , ref Mumap, "Mumap", optional: true);
            ctx.Set(GYML_REF, ref StageLoadInfo!, "StageLoadInfo", optional: true);
            ctx.Set(GYML_REF, ref StageSequenceInfo!, "StageSequenceInfo", optional: true);
            ctx.Set(GYML_REF, ref StaticCompoundBodySourceParam!, "StaticCompoundBodySourceParam", optional: true);
            ctx.Set(GYML_REF, ref WorldList!, "WorldList", optional: true);
            ctx.Set(GYML_REF, ref WorldMapAnalysisInfo!, "WorldMapAnalysisInfo", optional: true);
            ctx.Set(GYML_REF, ref WorldMapInfo!, "WorldMapInfo", optional: true);
        }
    }
}