using System.ComponentModel;
using Fushigi.Data;
using Fushigi.Data.Files;
using Fushigi.Data.Files.GymlTypes;
using Fushigi.Data.RomFSExtensions;

namespace Fushigi.Logic.Stages;

public record StageComponentMissingErrorInfo(string ComponentName, StageParam.StageCategory? StageCategory, 
    RomFS.RetrievedFileLocation StageParamFileLocation);
public record UnexpectedStageCategoryErrorInfo(StageParam.StageCategory StageCategory, 
    RomFS.RetrievedFileLocation StageParamFileLocation,
    RomFS.RetrievedFileLocation? ReferencingFileLocationInfo = null);
public interface IStageLoadingErrorHandler
: IGymlFileLoadingErrorHandler, IMapLoadingErrorHandler
{
    public Task OnStageParamComponentMissing(StageComponentMissingErrorInfo info);
    public Task OnUnexpectedStageCategory(UnexpectedStageCategoryErrorInfo info);
}

public abstract class Stage
{
    public MuMap? MuMap => _baseInfo.MuMap;
    
    public static async Task<bool> ValidateCategory(RomFS romFS, GymlRef<StageParam> stageParamRef, 
        StageParam.StageCategory expectedCategory,
        RomFS.RetrievedFileLocation? referencingFileLocation,
        IStageLoadingErrorHandler errorHandler)
    {
        if (await romFS.LoadGyml(stageParamRef, errorHandler)
            is not (true, { } stageParam)) return false;
        
        var refStageCategory = stageParam.Get(x=>ref x.Category);
        if (refStageCategory == expectedCategory) 
            return true;
        
        await errorHandler.OnUnexpectedStageCategory(new UnexpectedStageCategoryErrorInfo(
            refStageCategory, 
            StageParamFileLocation: romFS.GetLoadedGymlFileLocation(stageParamRef),
            ReferencingFileLocationInfo: referencingFileLocation));
        return true;
    } 
    
    protected RomFS RomFS => _baseInfo.RomFS;
    
    protected Stage(in StageBaseInfo baseInfo)
    {
        _baseInfo = baseInfo;
    }
    
    protected struct StageBaseInfo
    {
        public required RomFS RomFS;
        public required StageParam StageParam;
        public required GymlRef<StageParam> StageParamRef;
        public required MuMap? MuMap;
        
        public Task<(bool success, T componentRef)> GetComponent<T>(string componentName,
            GymlFile<StageParam>.SubValueAccessor<StageParam.StageComponents, T> accessor, 
            IStageLoadingErrorHandler errorHandler)
        => Stage.GetComponent(RomFS, StageParam, StageParamRef, componentName, accessor, errorHandler);
    }
    
    private static async Task<(bool success, T componentRef)> GetComponent<T>(
        RomFS romFS, StageParam stageParam, GymlRef<StageParam> stageParamRef,
        string componentName, GymlFile<StageParam>.SubValueAccessor<StageParam.StageComponents, T> accessor, 
        IStageLoadingErrorHandler errorHandler)
    {
        var stageCategory = stageParam.Get(x=>ref x.Category);

        if (stageParam.Get(
                x => ref x.Components,
                accessor) is { } componentRef) return (true, componentRef);
            
        await errorHandler.OnStageParamComponentMissing(new StageComponentMissingErrorInfo(
            componentName, stageCategory, romFS.GetLoadedGymlFileLocation(stageParamRef)));
            
        return (false, default!);

    }
    
    protected static async Task<(bool success, StageBaseInfo loadedInfo)> Load(
        RomFS romFS, GymlRef<StageParam> stageParamRef, MuMap.StageLoadContext stageLoadContext,
        Func<GymlRef<StageParam>, Task<bool>> refStageLoader,
        IStageLoadingErrorHandler errorHandler)
    {
        if (await romFS.LoadGyml(stageParamRef, errorHandler)
            is not (true, { } stageParam)) return (false, default);
        
        if (await GetComponent(romFS, stageParam, stageParamRef, 
                "MuMap", x => ref x._.Mumap, errorHandler) 
            is not (true, { } muMapRef)) return (false, default);
        
        if (await MuMap.Load(romFS, muMapRef, stageLoadContext, refStageLoader, errorHandler)
            is not (true, { } muMap)) return (false, default);
        
        var info = new StageBaseInfo
        {
            RomFS = romFS,
            StageParam = stageParam,
            StageParamRef = stageParamRef,
            MuMap = muMap
        };
        
        return (true, info);
    }
    
    protected static async Task<(bool success, StageBaseInfo loadedInfo)> LoadNoMap(
        RomFS romFS, GymlRef<StageParam> stageParamRef,
        IStageLoadingErrorHandler errorHandler)
    {
        if (await romFS.LoadGyml(stageParamRef, errorHandler)
            is not (true, { } stageParam)) return (false, default);
        
        var info = new StageBaseInfo
        {
            RomFS = romFS,
            StageParam = stageParam,
            StageParamRef = stageParamRef,
            MuMap = null
        };
        
        return (true, info);
    }
    
    private readonly StageBaseInfo _baseInfo;
}