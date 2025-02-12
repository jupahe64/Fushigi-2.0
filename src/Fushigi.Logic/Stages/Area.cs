using Fushigi.Data;
using Fushigi.Data.RomFSExtensions;

namespace Fushigi.Logic.Stages;

public sealed class Area : Stage
{
    public new static async Task<(bool success, Area? area)> Load(RomFS romFS, string stageParamGymlPath,
        IGymlFileLoadingErrorHandler errorHandler)
    {
        if (await Stage.Load(romFS, stageParamGymlPath, errorHandler)
            is not (true, var baseInfo)) return (false, default);
        
        var area = new Area(baseInfo);
        return (true, area);
    }
    
    private Area(in StageBaseInfo baseInfo) 
        : base(in baseInfo) { }
}