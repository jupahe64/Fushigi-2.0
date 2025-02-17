using Fushigi.Data;
using Fushigi.Data.Files;
using Fushigi.Data.Files.GymlTypes;
using Fushigi.Data.RomFSExtensions;

namespace Fushigi.Logic.Stages;

public interface IStageLoadingErrorHandler
: IGymlFileLoadingErrorHandler, IStageBcettFileLoadingErrorHandler
{
    
}

public abstract class Stage
{
    public MuMap MuMap => _baseInfo.MuMap;
    internal string GymlPath => _baseInfo.GymlPath;
    
    protected RomFS RomFS => _baseInfo.RomFS;
    
    protected Stage(in StageBaseInfo baseInfo)
    {
        _baseInfo = baseInfo;
    }
    
    protected struct StageBaseInfo
    {
        public required RomFS RomFS;
        public required string GymlPath;
        public required StageParam StageParam;
        public required MuMap MuMap;
    }
    
    protected static async Task<(bool success, StageBaseInfo loadedInfo)> Load(
        RomFS romFS, string stageParamGymlPath,
        IStageLoadingErrorHandler errorHandler)
    {
        if (await romFS.LoadGyml<StageParam>(stageParamGymlPath, errorHandler)
            is not (true, { } stageParam)) return (false, default);
        
        if (await MuMap.Load(romFS, stageParam.Components.Mumap!, errorHandler)
            is not (true, { } muMap)) return (false, default);
        
        var info = new StageBaseInfo
        {
            RomFS = romFS,
            GymlPath = stageParamGymlPath,
            StageParam = stageParam,
            MuMap = muMap
        };
        
        return (true, info);
    }
    
    private readonly StageBaseInfo _baseInfo;
}