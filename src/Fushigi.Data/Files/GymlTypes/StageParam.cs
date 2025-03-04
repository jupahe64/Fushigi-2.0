using System.Numerics;
using Fushigi.Data.BymlSerialization;

namespace Fushigi.Data.Files.GymlTypes;

public class StageParam : GymlFile<StageParam>, IGymlType
{
    public enum StageCategory
    {
        Invalid = 0,
        Area,
        Base,
        Course,
        Course1Area,
        Demo,
        Title,
        WorldMap
    }
    protected override StageParam This => this;
    public static string GymlTypeSuffix => "game__stage__StageParam";
    public static readonly string[] DefaultSavePath = ["Stage", "StageParam"];

    public INHERITED< StageComponents > Components;
    public INHERITED< StageCategory >   Category;

    protected override void Serialization<TContext>(TContext ctx)
    {
        base.Serialization(ctx);
        ctx.SetObject(ref Components, "Components");
        ctx.Set(SpecialConversions.GetStringEnumConversion<StageCategory>(), ref Category, "Category");
    }

    public class StageComponents : SerializableBymlObject<StageComponents>
    {
        // ReSharper disable once InconsistentNaming
        private static BymlConversion<StaticCompoundBodySourceParamRef> SCBSP_REF =>
            FileRefConversion.For<StaticCompoundBodySourceParamRef>();
        
        public INHERITED< GymlRef<AreaParam>?               > AreaParam;
        public INHERITED< GymlRef<BakeSource>?              > BakeSource;
        public INHERITED< GymlRef<CourseInfo>?              > CourseInfo;
        public INHERITED< GymlRef<DemoStageInfo>?           > DemoStageInfo;
        public INHERITED< GymlRef<MapAnalysisInfo>?         > MapAnalysisInfo;
        public INHERITED< MuMapRef?                         > Mumap;
        public INHERITED< GymlRef<StageLoadInfo>?           > StageLoadInfo;
        public INHERITED< GymlRef<StageSequenceInfo>?       > StageSequenceInfo;
        public INHERITED< StaticCompoundBodySourceParamRef? > StaticCompoundBodySourceParam;
        public INHERITED< GymlRef<WorldList>?               > WorldList;
        public INHERITED< GymlRef<WorldMapAnalysisInfo>?    > WorldMapAnalysisInfo;
        public INHERITED< GymlRef<WorldMapInfo>?            > WorldMapInfo;

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