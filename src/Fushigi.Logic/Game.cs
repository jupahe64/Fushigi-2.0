using System.Text.RegularExpressions;
using Fushigi.Data;
using Fushigi.Data.Files;
using Fushigi.Data.Files.GymlTypes;

namespace Fushigi.Logic;

public class Game
{
    public static async Task<(bool success, Game? loadedGame)> Load(string baseGameRomFSPath, string? modRomFSPath,
        Func<Task> onBaseGameAndModPathsIdentical,
        Func<RootDirectoryNotFoundErrorInfo, Task> onRootDirectoryNotFound,
        Func<MissingSubDirectoryErrorInfo, Task> onMissingSubDirectory,
        Func<MissingSystemFileErrorInfo, Task> onMissingSystemFile, 
        Func<FileDecompressionErrorInfo, Task> onFileDecompressionFailed,
        Func<FileFormatReaderErrorInfo, Task> onFileReadFailed)
    {
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (await RomFS.Load(baseGameRomFSPath, modRomFSPath,
                onBaseGameAndModPathsIdentical,
                onRootDirectoryNotFound,
                onMissingSubDirectory,
                onMissingSystemFile,
                onFileDecompressionFailed,
                onFileReadFailed)
            is (true, { } loadedRomFS))
        {
            return (true, new Game(loadedRomFS));
        }
        
        return (false, null);
    }

    public async Task<(bool success, CourseArea? loadedStage)> LoadCourse(
        string[] coursePath,
        Func<FilePathResolutionErrorInfo, Task> onFileNotFound, 
        Func<FileDecompressionErrorInfo, Task> onFileDecompressionFailed,
        Func<FileFormatReaderErrorInfo, Task> onFileReadFailed)
    {
        #region LoadCourse
        var (success, courseFile) = await _romFs.LoadFile(
            coursePath,
            FormatDescriptors.GetBcettFormat<CourseArea>(), 
            onFileNotFound, 
            onFileDecompressionFailed, 
            onFileReadFailed 
            );
        #endregion

        #region Get Stage/AreaParams
        if (success && courseFile.StageParamPath != null)
        {
            var stagePath = courseFile.StageParamPath.Split("/")[1..];
            stagePath[^1] = stagePath[^1].Replace(".gyml", ".bgyml");
            var (loadedStageParam, stageParam) = await _romFs.LoadFile(
                stagePath,
                FormatDescriptors.GetGymlFormat<StageParam>(), 
                onFileNotFound, 
                onFileDecompressionFailed, 
                onFileReadFailed 
                );

            if (loadedStageParam && stageParam.Components.ContainsKey("AreaParam"))
            {
                courseFile.StageParam = stageParam;
                var areaPath = ((string)stageParam.Components["AreaParam"]).Split("/")[1..];
                areaPath[^1] = areaPath[^1].Replace(".gyml", ".bgyml");

                var (loadedAreaParam, areaParam) = await _romFs.LoadFile(
                    areaPath,
                    FormatDescriptors.GetGymlFormat<AreaParam>(), 
                    onFileNotFound, 
                    onFileDecompressionFailed, 
                    onFileReadFailed 
                    );

                if (loadedAreaParam)
                    courseFile.AreaParam = areaParam;
            }
        }
        #endregion

        #region Get Areas with RefStages
        foreach (var stage in courseFile.RefStages ?? [])
        {
            var areaName = Regex.Match(stage, @"Course[^\.]*").Value;
            coursePath[^1] = areaName + ".bcett.byml.zs";
            var (loadedArea, area) = await LoadCourse(
                coursePath,
                onFileNotFound, 
                onFileDecompressionFailed, 
                onFileReadFailed);
                
            if (loadedArea)
                courseFile.Areas.Add(areaName, area);
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