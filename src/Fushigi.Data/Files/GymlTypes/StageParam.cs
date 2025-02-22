using Fushigi.Data.BymlSerialization;

namespace Fushigi.Data.Files.GymlTypes;

public class StageParam : GymlFile<StageParam>, IGymlType
{
    public static string GymlTypeSuffix => "game__stage__StageParam";
    public static readonly string[] DefaultSavePath = ["Stage", "StageParam"];

    public StageComponents Components = null!;

    protected override void Serialization<TContext>(TContext ctx)
    {
        base.Serialization(ctx);
        ctx.SetObject(ref Components, "Components");
    }

    public class StageComponents : SerializableBymlObject<StageComponents>
    {
        // ReSharper disable once InconsistentNaming
        private static BymlConversion<StaticCompoundBodySourceParamRef> SCBSP_REF =>
            FileRefConversion.For<StaticCompoundBodySourceParamRef>();
        
        public GymlRef<AreaParam>? AreaParam;
        public GymlRef<BakeSource>? BakeSource;
        public GymlRef<CourseInfo>? CourseInfo;
        public GymlRef<DemoStageInfo>? DemoStageInfo;
        public GymlRef<MapAnalysisInfo>? MapAnalysisInfo;
        public MuMapRef? Mumap;
        public GymlRef<StageLoadInfo>? StageLoadInfo;
        public GymlRef<StageSequenceInfo>? StageSequenceInfo;
        public StaticCompoundBodySourceParamRef? StaticCompoundBodySourceParam;
        public GymlRef<WorldList>? WorldList;
        public GymlRef<WorldMapAnalysisInfo>? WorldMapAnalysisInfo;
        public GymlRef<WorldMapInfo>? WorldMapInfo;

        protected override void Serialization<TContext>(TContext ctx)
        {
            ctx.Set(GYML_REF<AreaParam>(), ref AreaParam!, "AreaParam", optional: true);
            ctx.Set(GYML_REF<BakeSource>(), ref BakeSource!, "BakeSource", optional: true);
            ctx.Set(GYML_REF<CourseInfo>(), ref CourseInfo!, "CourseInfo", optional: true);
            ctx.Set(GYML_REF<DemoStageInfo>(), ref DemoStageInfo!, "DemoStageInfo", optional: true);
            ctx.Set(GYML_REF<MapAnalysisInfo>(), ref MapAnalysisInfo!, "MapAnalysisInfo", optional: true);
            ctx.Set(MU_MAP_REF , ref Mumap, "Mumap", optional: true);
            ctx.Set(GYML_REF<StageLoadInfo>(), ref StageLoadInfo!, "StageLoadInfo", optional: true);
            ctx.Set(GYML_REF<StageSequenceInfo>(), ref StageSequenceInfo!, "StageSequenceInfo", optional: true);
            ctx.Set(SCBSP_REF, ref StaticCompoundBodySourceParam!, "StaticCompoundBodySourceParam", optional: true);
            ctx.Set(GYML_REF<WorldList>(), ref WorldList!, "WorldList", optional: true);
            ctx.Set(GYML_REF<WorldMapAnalysisInfo>(), ref WorldMapAnalysisInfo!, "WorldMapAnalysisInfo", optional: true);
            ctx.Set(GYML_REF<WorldMapInfo>(), ref WorldMapInfo!, "WorldMapInfo", optional: true);
        }
    }
}