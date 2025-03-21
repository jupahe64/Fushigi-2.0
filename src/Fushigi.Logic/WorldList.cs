﻿using Fushigi.Data;
using Fushigi.Data.Files;
using Fushigi.Data.RomFSExtensions;
using Fushigi.Logic.Stages;

namespace Fushigi.Logic;

using GymlTypes = Data.Files.GymlTypes;

public class WorldList
{
    public static async Task<(bool success, WorldList? worldList)> Load(
        RomFS romFS, GymlRef<GymlTypes.WorldList> gymlPath,
        IStageLoadingErrorHandler errorHandler)
    {
        if (await romFS.LoadGyml(gymlPath, errorHandler)
            is not (true, { } worldListGyml)) return (false, null);
        
        var worldList = new WorldList(gymlPath, worldListGyml);
        
        foreach (var refStageGymlPath in worldListGyml.Get(x=>ref x.WorldMapStagePath))
        {
            if (refStageGymlPath.GymlRef is null)
            {
                worldList._worldMaps.Add(null);
                continue;
            }
            
            await Stage.ValidateCategory(romFS, refStageGymlPath.GymlRef.Value, 
                GymlTypes.StageParam.StageCategory.WorldMap, 
                romFS.GetLoadedGymlFileLocation(gymlPath), errorHandler);
            
            if (await WorldMap.Load(romFS, refStageGymlPath.GymlRef.Value, errorHandler)
                is not (true, {} worldMap)) return (false, null);
            
            worldList._worldMaps.Add(worldMap);
        }
        
        return (true, worldList);

    }
    
    public IReadOnlyList<WorldMap?> Worlds => _worldMaps;
    
    private readonly List<WorldMap?> _worldMaps = [];
    private readonly GymlRef<GymlTypes.WorldList> _gymlRef;
    private readonly GymlTypes.WorldList _worldList;

    private WorldList(GymlRef<GymlTypes.WorldList> gymlRef, GymlTypes.WorldList worldList)
    {
        _gymlRef = gymlRef;
        _worldList = worldList;
    }
}