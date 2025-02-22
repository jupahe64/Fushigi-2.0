using Fushigi.Data;
using Fushigi.Data.Files;
using Fushigi.Data.Files.GymlTypes;
using Fushigi.Data.RomFSExtensions;

namespace Fushigi.Logic.Stages;

public sealed class WorldMap : Stage
{
    public new static async Task<(bool success, WorldMap? worldMap)> Load(RomFS romFS, GymlRef stageParamGymlRef,
        IStageLoadingErrorHandler errorHandler)
    {
        if (await Stage.Load(romFS, stageParamGymlRef, errorHandler)
            is not (true, var baseInfo)) return (false, default);
        
        if (await romFS.LoadGyml<WorldMapInfo>(baseInfo.StageParam.Components.WorldMapInfo!.Value, errorHandler)
            is not (true, {} worldMapInfo)) return (false, default);
        
        var worldMap = new WorldMap(baseInfo, worldMapInfo);
        return (true, worldMap);
    }
    
    public IEnumerable<string> CourseKeys => _worldMapInfo.CourseTable.Select(x=>x.Key);

    public async Task<(bool success, Course? course)> LoadCourse(int courseIndex, 
        IStageLoadingErrorHandler errorHandler)
    {
        var stagePath = _worldMapInfo.CourseTable[courseIndex].StagePath;
        if (await Course.Load(RomFS, stagePath, errorHandler)
            is not (true, var course)) return (false, null);
        
        return (true, course);
    }
    
    private readonly WorldMapInfo _worldMapInfo;
    
    private WorldMap(in StageBaseInfo baseInfo, WorldMapInfo worldMapInfo) 
        : base(in baseInfo)
    {
        _worldMapInfo = worldMapInfo;
    }
}