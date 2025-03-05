using Fushigi.Data;
using Fushigi.Data.Files;
using Fushigi.Data.Files.GymlTypes;
using Fushigi.Data.RomFSExtensions;
using Fushigi.Data.StageObjects;
using Fushigi.Logic.Stages.Objects;
using Fushigi.Logic.Utils;

namespace Fushigi.Logic.Stages;

using DATA_ERROR_FLAGS = MuMap.StageLoadContext.StageDataErrorFlags;
using STAGE_PARAM_REF = GymlRef<StageParam>;

public record InvalidMapDataErrorInfo(
    RomFS.RetrievedFileLocation BcettFileLocation,
    IReadOnlyCollection<(ulong hash, IReadOnlyCollection<int> actorIndices)> 
        ActorsWithDuplicateHashes,
    IReadOnlyCollection<(ulong hash, IReadOnlyCollection<int> railIndices)> 
        RailsWithDuplicateHashes,
    IReadOnlyCollection<(ulong hash, int railIdx, IReadOnlyCollection<int> pointInRailIndices)> 
        RailPointsWithDuplicateHashes,
    IReadOnlyCollection<(int linkIdx, ulong? invalidSourceHash, ulong? invalidDestHash)> 
        InvalidHashesInLinks,
    IReadOnlyCollection<(int railLinkIdx, ulong? invalidSrcActorHash, ulong? invalidDestRailHash, ulong? invalidPointHash)> 
        InvalidHashesInActorRailLinks,
    IReadOnlyCollection<(int groupIdx, IReadOnlyCollection<(int inGroupIdx, ulong actorHash)> invalidHashesInGroup)> 
        InvalidHashesInGroups,
    IReadOnlyCollection<(ulong actorHash, IReadOnlyCollection<(int groupIdx, int inGroupIdx)> groupAssignments)> 
        DuplicateActorGroupAssignments
);

public interface IMapLoadingErrorHandler : IStageBcettFileLoadingErrorHandler
{
    Task OnInvalidMapData(InvalidMapDataErrorInfo errorInfo);
}

public sealed class MuMap
{
    public class StageLoadContext
    {
        public readonly Dictionary<ulong, Actor> ActorLookUp = [];
        public readonly Dictionary<(ulong rail, ulong point), Rail.Point> RailPointLookUp = [];

        //for early error detection
        public readonly HashSet<ulong> UniqueRailHashes = [];
        
        [Flags]
        public enum StageDataErrorFlags
        {
            None = 0,
            DuplicateHash,
            MissingHashInLinksOrGroups,
            DuplicateActorGroupAssignment
        }

        public StageDataErrorFlags Errors;
    }
    
    public static async Task<(bool success, MuMap? muMap)> Load(
        RomFS romFS, MuMapRef muMapRef, StageLoadContext stageLoadContext, Func<STAGE_PARAM_REF, Task<bool>> refStageLoader,
        IMapLoadingErrorHandler errorHandler)
    {
        string[] bcettPath = FileRefConversion.GetRomFSFilePath(muMapRef);
        
        if (await romFS.LoadStageBcett(bcettPath, errorHandler)
                is not (true, var stageBcett, {} dataKeeper, {} fileLocation)) return (false, null);

        var muMap = new MuMap(muMapRef);
        foreach (var stageRef in dataKeeper.GetData(stageBcett.RefStages))
        {
            var errorsBefore = stageLoadContext.Errors;
            stageLoadContext.Errors = DATA_ERROR_FLAGS.None;
            
            bool success = await refStageLoader(stageRef);
            
            stageLoadContext.Errors = errorsBefore;
            
            if (!success)
                return (false, null);
        }
        foreach (var actorData in dataKeeper.GetData(stageBcett.Actors))
            muMap.AddActor(actorData, stageLoadContext);
        foreach (var railData in dataKeeper.GetData(stageBcett.Rails))
            muMap.AddRail(railData, dataKeeper, stageLoadContext);
        foreach (var bgUnitDataData in dataKeeper.GetData(stageBcett.BgUnits))
            muMap.AddBgUnit(bgUnitDataData, dataKeeper);
        
        foreach(var link in dataKeeper.GetData(stageBcett.Links))
        {
            if (stageLoadContext.ActorLookUp.TryGetValue(link.Source, out var srcActor) &&
                stageLoadContext.ActorLookUp.TryGetValue(link.Destination, out var destActor))
            {
                muMap.Links.Add((srcActor, destActor, link.LinkType));
            }
            else
                stageLoadContext.Errors |= DATA_ERROR_FLAGS.MissingHashInLinksOrGroups;
        }

        foreach(var link in dataKeeper.GetData(stageBcett.ActorToRailLinks))
        {
            if (stageLoadContext.ActorLookUp.TryGetValue(link.Source, out var srcActor) &&
                stageLoadContext.RailPointLookUp.TryGetValue((link.Destination, link.Point), out var destPoint))
            {
                muMap.ActorToRailLinks.Add((srcActor, destPoint, link.LinkType));
            }
            else
                stageLoadContext.Errors |= DATA_ERROR_FLAGS.MissingHashInLinksOrGroups;
        }

        foreach(var group in dataKeeper.GetData(stageBcett.SimultaneousGroups))
        {
            var newGroup = new SimultaneousGroup();
            foreach (ulong actorHash in dataKeeper.GetData(group.Actors))
            {
                if (!stageLoadContext.ActorLookUp.TryGetValue(actorHash, out var actor))
                {
                    stageLoadContext.Errors |= DATA_ERROR_FLAGS.MissingHashInLinksOrGroups;
                    continue;
                }

                if (actor.SimultaneousGroup != null)
                    stageLoadContext.Errors |= DATA_ERROR_FLAGS.DuplicateActorGroupAssignment;

                actor.SimultaneousGroup = newGroup;

            }
        }

        if (!await ValidateMapDataAndReportErrors(fileLocation, stageLoadContext, stageBcett, dataKeeper, errorHandler))
            return (false, null);

        return (true, muMap);
    }
    
    private readonly MuMapRef _muMapRef;
    public List<Actor> Actors = [];
    public List<Rail> Rails = [];
    public List<BGUnit> BgUnits = [];
    public List<(Actor source, Actor dest, string name)> Links = [];
    public List<(Actor source, Rail.Point dest, string name)> ActorToRailLinks = [];

    private MuMap(MuMapRef muMapPath)
    {
        _muMapRef = muMapPath;
    }

    public class SimultaneousGroup;

    private void AddActor(StageActorData actorData, StageLoadContext stageLoadContext)
    {
        var actor = new Actor{
            Gyaml = actorData.Gyaml,
            Name = actorData.Name,
            Layer = actorData.Layer,
            Dynamic = actorData.Dynamic,
            Rotate = actorData.Rotate,
            Scale = actorData.Scale,
            Translate = actorData.Translate
        };
        if (!stageLoadContext.ActorLookUp.TryAdd(actorData.Hash, actor))
            stageLoadContext.Errors |= DATA_ERROR_FLAGS.DuplicateHash;
        
        Actors.Add(actor);
    }

    private void AddRail(StageRailData railData, IDeserializedStageDataKeeper dataKeeper, StageLoadContext stageLoadContext)
    {
        var rail = new Rail{
            Points = new List<Rail.Point>(railData.Points.Count),
            Gyaml = railData.Gyaml,
            IsClosed = railData.IsClosed,
            Dynamic = railData.Dynamic,
        };
        foreach(var point in dataKeeper.GetData(railData.Points))
        {
            var newPoint = new Rail.Point{
                Dynamic = point.Dynamic,
                Translate = point.Translate,
                Control1 = point.Control1
            };
            rail.Points.Add(newPoint);
            
            if (!stageLoadContext.RailPointLookUp.TryAdd((railData.Hash, point.Hash), newPoint))
                stageLoadContext.Errors |= DATA_ERROR_FLAGS.DuplicateHash;
        }
        if (!stageLoadContext.UniqueRailHashes.Add(railData.Hash))
            stageLoadContext.Errors |= DATA_ERROR_FLAGS.DuplicateHash;
        
        Rails.Add(rail);
    }
    private void AddBgUnit(CourseBgUnitData bgUnitData, IDeserializedStageDataKeeper dataKeeper)
    {
        var bGUnit = new BGUnit{
            ModelType = (BGUnit.ModelTypes)bgUnitData.ModelType,
            SkinDivision = bgUnitData.SkinDivision,
            Walls = new List<BGUnit.Wall>(bgUnitData.Walls?.Count ?? 0),
            BeltRails = new List<BGUnit.Rail>(bgUnitData.BeltRails?.Count ?? 0),
        };
        foreach(var wall in dataKeeper.GetData(bgUnitData.Walls))
        {
            var newWall = new BGUnit.Wall{
                ExternalRail = CreateBgUnitRail(dataKeeper.GetData(wall.ExternalRail), dataKeeper),
                InternalRails = new List<BGUnit.Rail>(wall.InternalRails?.Count ?? 0)
            };
            foreach (var rail in dataKeeper.GetData(wall.InternalRails))
            {
                newWall.InternalRails.Add(CreateBgUnitRail(rail, dataKeeper));
            }
        }
        foreach (var rail in dataKeeper.GetData(bgUnitData.BeltRails))
        {
            bGUnit.BeltRails.Add(CreateBgUnitRail(rail, dataKeeper));
        }
        BgUnits.Add(bGUnit);
    }

    private BGUnit.Rail CreateBgUnitRail(CourseBgUnitData.Rail bgUnitRailData, IDeserializedStageDataKeeper dataKeeper)
    {
        var rail = new BGUnit.Rail{
            Points = new List<BGUnit.Point>(bgUnitRailData.Points.Count),
            IsClosed = bgUnitRailData.IsClosed,
        };
        foreach(var point in dataKeeper.GetData(bgUnitRailData.Points))
        {
            var newPoint = new BGUnit.Point{
                Translate = point.Translate,
            };
            rail.Points.Add(newPoint);
        }
        return rail;
    }

    private static async Task<bool> ValidateMapDataAndReportErrors(RomFS.RetrievedFileLocation bcettFileLocation, 
        StageLoadContext stageLoadContext, 
        StageBcett.Content bcett, IDeserializedStageDataKeeper dataKeeper,
        IMapLoadingErrorHandler errorHandler)
    {
        if (stageLoadContext.Errors == DATA_ERROR_FLAGS.None)
            return true;
        
        List<(ulong hash, IReadOnlyCollection<int> actorIndices)>? actorsWithDuplicateHashes = null;
        List<(ulong hash, IReadOnlyCollection<int> railIndices)>? railsWithDuplicateHashes = null;
        List<(ulong hash, int railIdx, IReadOnlyCollection<int> pointInRailIndices)>? railPointsWithDuplicateHashes = null;
        List<(int linkIdx, ulong? invalidSourceHash, ulong? invalidDestHash)>? invalidHashesInLinks = null;
        List<(int railLinkIdx, ulong? invalidSrcActorHash, ulong? invalidDestRailHash, ulong? invalidPointHash)>? invalidHashesInActorRailLinks = null;
        List<(int groupIdx, IReadOnlyCollection<(int inGroupIdx, ulong actorHash)> invalidHashesInGroup)>? invalidHashesInGroups = null;
        List<(ulong actorHash, IReadOnlyCollection<(int groupIdx, int inGroupIdx)> groupAssignments)>? duplicateActorGroupAssignments = null;
        
        //actors
        if (stageLoadContext.Errors.HasFlag(DATA_ERROR_FLAGS.DuplicateHash))
        {
            var actorIndicesWithSameHash = new Dictionary<ulong, List<int>>();

            var actorIdx = 0;
            foreach (var actor in dataKeeper.GetData(bcett.Actors))
            {
                actorIndicesWithSameHash.GetOrCreate(actor.Hash, () => []).Add(actorIdx);
                actorIdx++;
            }
        
            actorsWithDuplicateHashes = actorIndicesWithSameHash
                .Where(x=>x.Value.Count > 1)
                .Select(x=> (x.Key, (IReadOnlyCollection<int>)x.Value))
                .ToList();
        }

        //rails
        if (stageLoadContext.Errors.HasFlag(DATA_ERROR_FLAGS.DuplicateHash))
        {
            var railIndicesWithSameHash = new Dictionary<ulong, List<int>>();
            var railPointIndicesWithSameHash = new Dictionary<(ulong hash, int railIdx), List<int>>();

            var railIdx = 0;
            foreach (var rail in dataKeeper.GetData(bcett.Rails))
            {
                railIndicesWithSameHash.GetOrCreate(rail.Hash, () => []).Add(railIdx);

                var pointIdx = 0;
                foreach (var point in dataKeeper.GetData(bcett.Rails))
                {
                    railPointIndicesWithSameHash.GetOrCreate((point.Hash, railIdx), () => []).Add(pointIdx);
                    pointIdx++;
                }
                
                railIdx++;
            }
        
            railsWithDuplicateHashes = railIndicesWithSameHash
                .Where(x=>x.Value.Count > 1)
                .Select(x=> (x.Key, (IReadOnlyCollection<int>)x.Value))
                .ToList();
            
            railPointsWithDuplicateHashes = railPointIndicesWithSameHash
                .Where(x=>x.Value.Count > 1)
                .Select(x=> (x.Key.hash, x.Key.railIdx, (IReadOnlyCollection<int>)x.Value))
                .ToList();
        }

        //links
        if (stageLoadContext.Errors.HasFlag(DATA_ERROR_FLAGS.MissingHashInLinksOrGroups))
        {
            invalidHashesInLinks = [];

            var linkIdx = 0;
            foreach (var link in dataKeeper.GetData(bcett.Links))
            {
                ulong src = link.Source, dest = link.Destination;
                ulong? invalidSourceHash = !stageLoadContext.ActorLookUp.ContainsKey(src) ? src
                    : null;
                ulong? invalidDestinationHash = !stageLoadContext.ActorLookUp.ContainsKey(dest) ? dest
                    : null;
                
                if (invalidSourceHash.HasValue || invalidDestinationHash.HasValue)
                    invalidHashesInLinks.Add((linkIdx, invalidSourceHash, invalidDestinationHash));
                
                linkIdx++;
            }
        }
        
        //actor rail links
        if (stageLoadContext.Errors.HasFlag(DATA_ERROR_FLAGS.MissingHashInLinksOrGroups))
        {
            invalidHashesInActorRailLinks = [];

            var linkIdx = 0;
            foreach (var link in dataKeeper.GetData(bcett.ActorToRailLinks))
            {
                ulong src = link.Source, dest = link.Destination, point = link.Point;
                
                ulong? invalidSrcActorHash = !stageLoadContext.ActorLookUp.ContainsKey(src) ? src
                    : null;

                (ulong? invalidDestRailHash, ulong? invalidPointHash) = 
                    !stageLoadContext.RailPointLookUp.ContainsKey((dest, point)) ? (dest, point)
                        : ((ulong?)null, (ulong?)null);

                if (invalidDestRailHash != null && stageLoadContext.RailPointLookUp
                        .Keys.Any(x => x.rail == invalidDestRailHash.Value))
                    invalidDestRailHash = null; //destination is a valid rail, but point is still invalid
                
                if (invalidSrcActorHash.HasValue || invalidDestRailHash.HasValue || invalidPointHash.HasValue)
                    invalidHashesInActorRailLinks.Add((linkIdx, invalidSrcActorHash, invalidDestRailHash, invalidPointHash));
                
                linkIdx++;
            }
        }
        
        //simultaneous groups
        if (stageLoadContext.Errors.HasFlag(DATA_ERROR_FLAGS.MissingHashInLinksOrGroups))
        {
            invalidHashesInGroups = [];

            var groupIdx = 0;
            foreach (var group in dataKeeper.GetData(bcett.SimultaneousGroups))
            {
                List<(int inGroupIdx, ulong hash)>? list = null;
                var inGroupIdx = 0;
                foreach (ulong actorHash in dataKeeper.GetData(group.Actors))
                {
                    if (!stageLoadContext.ActorLookUp.ContainsKey(actorHash))
                    {
                        list ??= [];
                        list.Add((inGroupIdx, actorHash));
                    }

                    inGroupIdx++;
                }
                
                if (list != null)
                    invalidHashesInGroups.Add((groupIdx, list));
                
                groupIdx++;
            }
        }

        if (stageLoadContext.Errors.HasFlag(DATA_ERROR_FLAGS.DuplicateActorGroupAssignment))
        {
            Dictionary<ulong, List<(int groupIdx, int inGroupIdx)>> actorGroupAssignments = [];
            var groupIdx = 0;
            foreach (var group in dataKeeper.GetData(bcett.SimultaneousGroups))
            {
                var inGroupIdx = 0;
                foreach (ulong actorHash in dataKeeper.GetData(group.Actors))
                {
                    actorGroupAssignments.GetOrCreate(actorHash, () => []).Add((groupIdx, inGroupIdx));
                    inGroupIdx++;
                }
                groupIdx++;
            }
            
            duplicateActorGroupAssignments = actorGroupAssignments
                .Where(x=>x.Value.Count > 1)
                .Select(x => (x.Key, (IReadOnlyCollection<(int, int)>)x.Value))
                .ToList();
        }

        await errorHandler.OnInvalidMapData(new InvalidMapDataErrorInfo(
            bcettFileLocation,
            actorsWithDuplicateHashes ?? [],
            railsWithDuplicateHashes ?? [],
            railPointsWithDuplicateHashes ?? [],
            invalidHashesInLinks ?? [],
            invalidHashesInActorRailLinks ?? [],
            invalidHashesInGroups ?? [],
            duplicateActorGroupAssignments ?? []
        ));
        
        return false;
    }
}