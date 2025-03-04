using Fushigi.Data;
using Fushigi.Data.Files;
using Fushigi.Data.Files.GymlTypes;

namespace Fushigi.Logic.Stages;

public sealed class Area : Stage
{
    public new static async Task<(bool success, Area? area)> Load(RomFS romFS, GymlRef<StageParam> stageParamGymlRef, MuMap.StageLoadContext stageLoadContext,
        IStageLoadingErrorHandler errorHandler)
    {
        if (await Stage.Load(romFS, stageParamGymlRef, stageLoadContext, _ => Task.FromResult(false), errorHandler)
            is not (true, var baseInfo)) return (false, default);
        
        var area = new Area(baseInfo);
        return (true, area);
    }
    
    private Area(in StageBaseInfo baseInfo) 
        : base(in baseInfo) { }
}