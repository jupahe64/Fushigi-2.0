using Fushigi.Data;
using Fushigi.Data.Files;
using Fushigi.Data.Files.GymlTypes;
using Fushigi.Data.RomFSExtensions;

namespace Fushigi.Logic.Stages;

public static class StageLoading
{
    public static async Task<(bool success, Stage? stage)> Load(
        RomFS romFS, GymlRef<StageParam> stageParamRef,
        IStageLoadingErrorHandler errorHandler)
    {
        if (await romFS.LoadGyml(stageParamRef, errorHandler)
            is not (true, { } stageParam)) return (false, default);
        
        var category = stageParam.Get(x=>ref x.Category);
        switch (category)
        {
            case StageParam.StageCategory.Course:
                return await Course.Load(romFS, stageParamRef, isSingleAreaCourse: false, errorHandler);
            case StageParam.StageCategory.Course1Area:
                return await Course.Load(romFS, stageParamRef, isSingleAreaCourse: true, errorHandler);
            case StageParam.StageCategory.Demo:
                break; //TODO
            case StageParam.StageCategory.Title:
                break; //TODO
            case StageParam.StageCategory.WorldMap:
                return await WorldMap.Load(romFS, stageParamRef, errorHandler);
            
            case StageParam.StageCategory.Area:
            case StageParam.StageCategory.Base:
                break;
            case StageParam.StageCategory.Invalid:
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        errorHandler.OnUnexpectedStageCategory(new UnexpectedStageCategoryErrorInfo(
            category, romFS.GetLoadedGymlFileLocation(stageParamRef)));
        return (false, default);
    }
}