using Fushigi.Data;
using Fushigi.Data.Files;
using Fushigi.Data.Files.GymlTypes;
using Fushigi.Data.RomFSExtensions;

namespace Fushigi.Logic.Stages;

public sealed class Area : Stage
{
    public AreaParam AreaParam { get; }

    public static async Task<(bool success, Area? area)> Load(RomFS romFS, GymlRef<StageParam> stageParamRef, 
        MuMap.StageLoadContext stageLoadContext,
        IStageLoadingErrorHandler errorHandler)
    {
        if (await Stage.Load(romFS, stageParamRef, stageLoadContext, 
                refStageLoader: _ => Task.FromResult(true), //ignore ref stages in area
                errorHandler)
            is not (true, var baseInfo)) return (false, default);

        
        if (await baseInfo.GetComponent("AreaParam", x => ref x._.AreaParam, errorHandler) 
            is not (true, { } areaParamRef)) return (false, default);
        
        if (await romFS.LoadGyml(areaParamRef, errorHandler) is not (true, { } areaParam))
            return (false, default);
        
        var area = new Area(baseInfo, areaParam);
        return (true, area);
    }
    
    private Area(in StageBaseInfo baseInfo, AreaParam areaParam) 
        : base(in baseInfo)
    {
        AreaParam = areaParam;
    }
}