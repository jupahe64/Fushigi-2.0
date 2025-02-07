using System.Text.RegularExpressions;
using Fushigi.Data;
using Fushigi.Data.Files;
using Fushigi.Data.Files.GymlTypes;
using Fushigi.Data.RomFSExtensions;

namespace Fushigi.Logic;

public class Game
{
    public static async Task<(bool success, Game? loadedGame)> Load(string baseGameRomFSPath, string? modRomFSPath,
        IRomFSLoadingErrorHandler errorHandler)
    {
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (await RomFS.Load(baseGameRomFSPath, modRomFSPath,
                errorHandler)
            is (true, { } loadedRomFS))
        {
            return (true, new Game(loadedRomFS));
        }
        
        return (false, null);
    }

    public async Task<(bool success, StageBcett? loadedStage)> LoadCourse(
        string[] coursePath,
        IGymlFileLoadingErrorHandler errorHandler)
    {
        #region LoadCourse
        var (success, courseFile) = await _romFs.LoadFile(
            coursePath,
            FormatDescriptors.GetBcettFormat<StageBcett>(), 
            errorHandler);
        #endregion

        #region Get Stage/AreaParams
        if (success && courseFile.StageParamPath != null)
        {
            var (loadedStageParam, stageParam) = await _romFs.LoadGyml<StageParam>(
                courseFile.StageParamPath, 
                errorHandler);

            if (loadedStageParam && stageParam.Components.ContainsKey("AreaParam"))
            {
                var areaPath = (string)stageParam.Components["AreaParam"];

                var (loadedAreaParam, areaParam) = await _romFs.LoadGyml<AreaParam>(
                    areaPath,
                    errorHandler);
            }
        }
        #endregion

        #region Get Areas with RefStages
        foreach (var stage in courseFile.RefStages ?? [])
        {
            var areaName = Regex.Match(stage, @"Course[^\.]*").Value;
            coursePath[^1] = areaName + ".bcett.byml.zs";
            var (loadedArea, area) = await LoadCourse(coursePath, errorHandler);
        }
        #endregion
        return (success, courseFile);
    }
    
    private readonly RomFS _romFs;

    private Game(RomFS romFs)
    {
        _romFs = romFs;
    }
}