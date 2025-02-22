using Fushigi.Data;
using Fushigi.Data.Files;
using Fushigi.Data.RomFSExtensions;
using Fushigi.Logic.Stages;

namespace Fushigi.Logic;

using GymlTypes = Data.Files.GymlTypes;

public class WorldList
{
    public static async Task<(bool success, WorldList? muMap)> Load(
        RomFS romFS, GymlRef gymlPath,
        IStageLoadingErrorHandler errorHandler)
    {
        if (await romFS.LoadGyml<GymlTypes.WorldList>(gymlPath, errorHandler)
            is not (true, { } worldListGyml)) return (false, null);
        
        var worldList = new WorldList(gymlPath, worldListGyml);
        
        foreach (var refStageGymlPath in worldListGyml.WorldMapStagePath)
        {
            if (refStageGymlPath.GymlRef is null)
            {
                worldList._worldMaps.Add(null);
                continue;
            }
            
            if (await WorldMap.Load(romFS, refStageGymlPath.GymlRef.Value, errorHandler)
                is not (true, {} worldMap)) return (false, null);
            
            worldList._worldMaps.Add(worldMap);
        }
        
        return (true, worldList);

    }
    
    public IReadOnlyList<WorldMap?> Worlds => _worldMaps;
    
    private readonly List<WorldMap?> _worldMaps = [];
    private readonly GymlRef _gymlRef;
    private readonly GymlTypes.WorldList _worldList;

    private WorldList(GymlRef gymlRef, GymlTypes.WorldList worldList)
    {
        _gymlRef = gymlRef;
        _worldList = worldList;
    }
}