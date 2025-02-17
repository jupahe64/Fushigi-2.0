using Fushigi.Data;
using Fushigi.Data.Files;
using Fushigi.Data.RomFSExtensions;
using Fushigi.Logic.Stages;

namespace Fushigi.Logic;

using GymlTypes = Data.Files.GymlTypes;

public class WorldList
{
    public static async Task<(bool success, WorldList? muMap)> Load(
        RomFS romFS, string gymlPath,
        IStageLoadingErrorHandler errorHandler)
    {
        if (await romFS.LoadGyml<GymlTypes.WorldList>(gymlPath, errorHandler)
            is not (true, { } worldListGyml)) return (false, null);
        
        var worldList = new WorldList(gymlPath, worldListGyml);
        
        foreach (string refStageGymlPath in worldListGyml.WorldMapStagePath)
        {
            if (string.IsNullOrEmpty(refStageGymlPath))
                continue;
            
            if (await WorldMap.Load(romFS, refStageGymlPath, errorHandler)
                is not (true, {} worldMap)) return (false, null);
            
            worldList._worldMaps.Add(worldMap);
        }
        
        return (true, worldList);

    }
    
    public IReadOnlyList<WorldMap> Worlds => _worldMaps;
    
    private readonly List<WorldMap> _worldMaps = [];
    private readonly string _gymlPath;
    private readonly GymlTypes.WorldList _worldList;

    private WorldList(string gymlPath, GymlTypes.WorldList worldList)
    {
        _gymlPath = gymlPath;
        _worldList = worldList;
    }
}