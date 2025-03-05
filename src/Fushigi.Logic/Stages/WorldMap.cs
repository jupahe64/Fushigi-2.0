using Fushigi.Data;
using Fushigi.Data.Files;
using Fushigi.Data.Files.GymlTypes;
using Fushigi.Data.RomFSExtensions;

namespace Fushigi.Logic.Stages;

public sealed class WorldMap : Stage
{
    public static async Task<(bool success, WorldMap? worldMap)> Load(RomFS romFS, 
        GymlRef<StageParam> stageParamRef,
        IStageLoadingErrorHandler errorHandler)
    {
        var stageLoadContext = new MuMap.StageLoadContext();
        if (await Stage.Load(romFS, stageParamRef, stageLoadContext, 
                refStageLoader: _ => Task.FromResult(true), //ignore ref stages in worldMap
                errorHandler)
            is not (true, var baseInfo)) return (false, default);

        
        if (await baseInfo.GetComponent("WorldMapInfo", x => ref x._.WorldMapInfo, errorHandler) 
            is not (true, { } worldMapInfoRef)) return (false, default);
        
        if (await romFS.LoadGyml(worldMapInfoRef, errorHandler)
            is not (true, {} worldMapInfo)) return (false, default);
        
        var worldMap = new WorldMap(baseInfo, worldMapInfo);
        return (true, worldMap);
    }

    public IEnumerable<(string, string)> CourseKeys => _worldMapInfo.Get(x=>ref x.CourseTable).Select(x=>(x.Key, x.StagePath.ValidatedRefPath));

    public async Task<(bool success, Stage? stage)> LoadCourse(int courseIndex, 
        IStageLoadingErrorHandler errorHandler)
    {
        var stagePath = _worldMapInfo.Get(x=>ref x.CourseTable)[courseIndex].StagePath;
        if (await StageLoading.Load(RomFS, stagePath, errorHandler)
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