using Fushigi.Data;

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
    
    private readonly RomFS _romFs;

    private Game(RomFS romFs)
    {
        _romFs = romFs;
    }
}