using Fushigi.Data.BymlSerialization;

namespace Fushigi.Data.Files.GymlTypes;

public class WorldMapInfo : GymlFile<WorldMapInfo>
{
    public List<CourseTableEntry> CourseTable;
    public class CourseTableEntry : SerializableBymlObject<CourseTableEntry>
    {
        public string Key = null!;
        public GymlRef StagePath;

        protected override void Serialization<TContext>(TContext ctx)
        {
            ctx.Set(STRING, ref Key, "Key");
            ctx.Set(GYML_REF, ref StagePath, "StagePath");
        }
    }

    protected override void Serialization<TContext>(TContext ctx)
    {
        base.Serialization(ctx);
        ctx.SetArray(ref CourseTable, "CourseTable", CourseTableEntry.Conversion);
    }
}