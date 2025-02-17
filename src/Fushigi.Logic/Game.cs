using Fushigi.Data;
using Fushigi.Logic.Stages;

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

    public Task<(bool success, WorldList? worldList)> LoadWorldList(
        IStageLoadingErrorHandler errorHandler)
        => WorldList.Load(_romFS, "Work/Stage/WorldList/WorldList.game__stage__WorldList.gyml", errorHandler);

    private readonly RomFS _romFS;

    private Game(RomFS romFs)
    {
        _romFS = romFs;
    }
}