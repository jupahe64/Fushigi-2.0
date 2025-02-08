using Fushigi.Data.BymlSerialization;

namespace Fushigi.Data.Files.GymlTypes;

public class StageParam : GymlFile<StageParam>
{
    public static readonly string GymlTypeSuffix = "game__stage__StageParam";
    public static readonly string[] DefaultSavePath = ["Stage", "StageParam"];

    public StageComponents Components = null!;
    protected override void Deserialize(Deserializer d)
    {
        base.Deserialize(d);
        d.SetObject(ref Components, "Components");
    }

    protected override void Serialize(Serializer s)
    {
        base.Serialize(s);
        s.SetObject(ref Components, "Components");
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
        
        protected override void Deserialize(Deserializer d)
        {
            d.SetString(ref AreaParam!, "AreaParam", optional: true);
            d.SetString(ref BakeSource!, "BakeSource", optional: true);
            d.SetString(ref CourseInfo!, "CourseInfo", optional: true);
            d.SetString(ref MapAnalysisInfo!, "MapAnalysisInfo", optional: true);
            d.SetString(ref Mumap!, "Mumap", optional: true);
            d.SetString(ref StageLoadInfo!, "StageLoadInfo", optional: true);
            d.SetString(ref StageSequenceInfo!, "StageSequenceInfo", optional: true);
            d.SetString(ref StaticCompoundBodySourceParam!, "StaticCompoundBodySourceParam", optional: true);
            d.SetString(ref WorldList!, "WorldList", optional: true);
            d.SetString(ref WorldMapAnalysisInfo!, "WorldMapAnalysisInfo", optional: true);
            d.SetString(ref WorldMapInfo!, "WorldMapInfo", optional: true);
        }

        protected override void Serialize(Serializer s)
        {
            s.SetString(ref AreaParam!, "AreaParam", optional: true);
            s.SetString(ref BakeSource!, "BakeSource", optional: true);
            s.SetString(ref CourseInfo!, "CourseInfo", optional: true);
            s.SetString(ref MapAnalysisInfo!, "MapAnalysisInfo", optional: true);
            s.SetString(ref Mumap!, "Mumap", optional: true);
            s.SetString(ref StageLoadInfo!, "StageLoadInfo", optional: true);
            s.SetString(ref StageSequenceInfo!, "StageSequenceInfo", optional: true);
            s.SetString(ref StaticCompoundBodySourceParam!, "StaticCompoundBodySourceParam", optional: true);
            s.SetString(ref WorldList!, "WorldList", optional: true);
            s.SetString(ref WorldMapAnalysisInfo!, "WorldMapAnalysisInfo", optional: true);
            s.SetString(ref WorldMapInfo!, "WorldMapInfo", optional: true);
        }
    }
}