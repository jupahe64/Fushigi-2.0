namespace Fushigi.Data.Files.GymlTypes;

#region game.Stage
public class BakeSource : GymlFile<BakeSource>, IGymlType
{
    protected override BakeSource This => this;
    public static string GymlTypeSuffix => "game__stage__BakeSource";
    public static readonly string[] DefaultSavePath = ["Stage", "BakeSource"];
}

public class CourseInfo : GymlFile<CourseInfo>, IGymlType
{
    protected override CourseInfo This => this;
    public static string GymlTypeSuffix => "game__stage__CourseInfo";
    public static readonly string[] DefaultSavePath = ["Stage", "CourseInfo"];
}

public class DemoStageInfo : GymlFile<DemoStageInfo>, IGymlType
{
    protected override DemoStageInfo This => this;
    public static string GymlTypeSuffix => "game__stage__DemoStageInfo";
    public static readonly string[] DefaultSavePath = ["Stage", "DemoStageInfo"];
}

public class MapAnalysisInfo : GymlFile<MapAnalysisInfo>, IGymlType
{
    protected override MapAnalysisInfo This => this;
    public static string GymlTypeSuffix => "game__stage__MapAnalysisInfo";
    public static readonly string[] DefaultSavePath = ["Stage", "MapAnalysisInfo"];
}

public class StageLoadInfo : GymlFile<StageLoadInfo>, IGymlType
{
    protected override StageLoadInfo This => this;
    public static string GymlTypeSuffix => "game__stage__StageLoadInfo";
    public static readonly string[] DefaultSavePath = ["Stage", "StageLoadInfo"];
}

public class StageSequenceInfo : GymlFile<StageSequenceInfo>, IGymlType
{
    protected override StageSequenceInfo This => this;
    public static string GymlTypeSuffix => "game__stage__StageSequenceInfo";
    public static readonly string[] DefaultSavePath = ["Stage", "StageSequenceInfo"];
}

public class WorldMapAnalysisInfo : GymlFile<WorldMapAnalysisInfo>, IGymlType
{
    protected override WorldMapAnalysisInfo This => this;
    public static string GymlTypeSuffix => "game__stage__WorldMapAnalysisInfo";
    public static readonly string[] DefaultSavePath = ["Stage", "WorldMapAnalysisInfo"];
}
#endregion