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
    internal GymlRef<StageParam> GymlRef => _baseInfo.GymlRef;
    
    protected RomFS RomFS => _baseInfo.RomFS;
    
    protected Stage(in StageBaseInfo baseInfo)
    {
        _baseInfo = baseInfo;
    }
    
    protected struct StageBaseInfo
    {
        public required RomFS RomFS;
        public required GymlRef<StageParam> GymlRef;
        public required StageParam StageParam;
        public required MuMap MuMap;
    }
    
    protected static async Task<(bool success, StageBaseInfo loadedInfo)> Load(
        RomFS romFS, GymlRef<StageParam> stageParamGymlRef,
        IStageLoadingErrorHandler errorHandler)
    {
        if (await romFS.LoadGyml(stageParamGymlRef, errorHandler)
            is not (true, { } stageParam)) return (false, default);
        
        if (await MuMap.Load(romFS, stageParam.Components.Mumap!.Value, errorHandler)
            is not (true, { } muMap)) return (false, default);
        
        var info = new StageBaseInfo
        {
            RomFS = romFS,
            GymlRef = stageParamGymlRef,
            StageParam = stageParam,
            MuMap = muMap
        };
        
        return (true, info);
    }
    
    private readonly StageBaseInfo _baseInfo;
}