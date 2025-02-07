using System.Text.RegularExpressions;
using Fushigi.Data;
using Fushigi.Data.Files;
using Fushigi.Data.Files.GymlTypes;
using Fushigi.Data.RomFSExtensions;

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

    public async Task<(bool success, StageBcett? loadedStage)> LoadCourse(
        string[] coursePath,
        Func<FilePathResolutionErrorInfo, Task> onFileNotFound, 
        Func<FileDecompressionErrorInfo, Task> onFileDecompressionFailed,
        Func<FileFormatReaderErrorInfo, Task> onFileReadFailed,
        Func<LoadedGymlTypeMismatchErrorInfo, Task> onGymlTypeMismatch,
        Func<InvalidFileRefPathErrorInfo, Task> onInvalidFileRefPath,
        Func<CyclicInheritanceErrorInfo, Task> onCyclicInheritance)
    {
        #region LoadCourse
        var (success, courseFile) = await _romFs.LoadFile(
            coursePath,
            FormatDescriptors.GetBcettFormat<StageBcett>(), 
            onFileNotFound, 
            onFileDecompressionFailed, 
            onFileReadFailed 
            );
        #endregion

        #region Get Stage/AreaParams
        if (success && courseFile.StageParamPath != null)
        {
            var (loadedStageParam, stageParam) = await _romFs.LoadGyml<StageParam>(
                courseFile.StageParamPath, 
                onFileNotFound, 
                onFileDecompressionFailed, 
                onFileReadFailed,
                onGymlTypeMismatch,
                onInvalidFileRefPath,
                onCyclicInheritance
                );

            if (loadedStageParam && stageParam.Components.ContainsKey("AreaParam"))
            {
                var areaPath = (string)stageParam.Components["AreaParam"];

                var (loadedAreaParam, areaParam) = await _romFs.LoadGyml<AreaParam>(
                    areaPath,
                    onFileNotFound, 
                    onFileDecompressionFailed, 
                    onFileReadFailed,
                    onGymlTypeMismatch,
                    onInvalidFileRefPath,
                    onCyclicInheritance
                );
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
                onFileReadFailed,
                onGymlTypeMismatch,
                onInvalidFileRefPath,
                onCyclicInheritance);
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