using Fushigi.Data.BymlSerialization;

namespace Fushigi.Data.Files.GymlTypes;

public class WorldMapInfo : GymlFile<WorldMapInfo>, IGymlType
{
    protected override WorldMapInfo This => this;
    public static string GymlTypeSuffix => "game__stage__WorldMapInfo";
    public static readonly string[] DefaultSavePath = ["Stage", "WorldMapInfo"];

    public INHERITED< List<CourseTableEntry> > CourseTable;
    public class CourseTableEntry : SerializableBymlObject<CourseTableEntry>
    {
        public string Key = null!;
        public GymlRef<StageParam> StagePath;

        protected override void Serialization<TContext>(TContext ctx)
        {
            ctx.Set(STRING, ref Key, "Key");
            ctx.Set(GYML_REF<StageParam>(), ref StagePath, "StagePath");
        }
    }

    protected override void Serialization<TContext>(TContext ctx)
    {
        base.Serialization(ctx);
        ctx.SetArray(ref CourseTable, "CourseTable", CourseTableEntry.Conversion);
    }
}