using Fushigi.Data.BymlSerialization;

namespace Fushigi.Data.Files.GymlTypes;

public class WorldMapInfo : GymlFile<WorldMapInfo>
{
    public List<CourseTableEntry> CourseTable;
    public class CourseTableEntry : SerializableBymlObject<CourseTableEntry>
    {
        public string Key = null!;
        public string StagePath = null!;
        protected override void Deserialize(Deserializer d)
        {
            d.SetString(ref Key, "Key");
            d.SetString(ref StagePath, "StagePath");
        }

        protected override void Serialize(Serializer s)
        {
            s.SetString(ref Key, "Key");
            s.SetString(ref StagePath, "StagePath");
        }
    }
    
    protected override void Deserialize(Deserializer d)
    {
        base.Deserialize(d);
        d.SetArray(ref CourseTable, "CourseTable", CourseTableEntry.Conversion);
    }

    protected override void Serialize(Serializer s)
    {
        base.Serialize(s);
        s.SetArray(ref CourseTable, "CourseTable", CourseTableEntry.Conversion);
    }
}