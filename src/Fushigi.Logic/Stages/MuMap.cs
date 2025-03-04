using System.Diagnostics;
using System.Drawing;
using Fushigi.Data;
using Fushigi.Data.Files;
using Fushigi.Data.Files.GymlTypes;
using Fushigi.Data.RomFSExtensions;
using Fushigi.Data.StageObjects;
using Fushigi.Logic.Stages.Objects;

namespace Fushigi.Logic.Stages;

using STAGE_PARAM_REF = GymlRef<StageParam>;

public sealed class MuMap
{
    public static async Task<(bool success, MuMap? muMap)> Load(
        RomFS romFS, MuMapRef muMapRef, StageLoadContext stageLoadContext, Func<GymlRef<StageParam>, Task<bool>> refStageLoader,
        IStageBcettFileLoadingErrorHandler errorHandler)
    {
        string[] bcettPath = FileRefConversion.GetRomFSFilePath(muMapRef);
        
        if (await romFS.LoadStageBcett(bcettPath, errorHandler)
                is not (true, var stageBcett, {} dataKeeper)) return (false, null);

        var muMap = new MuMap(muMapRef);
        foreach (STAGE_PARAM_REF stageRef in dataKeeper.GetData(stageBcett.RefStages))
        {
            if (!await refStageLoader(stageRef))
                return (false, null);
        }
        foreach (StageActorData actorData in dataKeeper.GetData(stageBcett.Actors))
            muMap.AddActor(actorData, stageLoadContext);
        foreach (StageRailData railData in dataKeeper.GetData(stageBcett.Rails))
            muMap.AddRail(railData, dataKeeper, stageLoadContext);
        foreach (CourseBgUnitData bgUnitDataData in dataKeeper.GetData(stageBcett.BgUnits))
            muMap.AddBGUnit(bgUnitDataData, dataKeeper);
        
        foreach(StageBcett.Link link in dataKeeper.GetData(stageBcett.Links))
            muMap.Links.Add((stageLoadContext.ActorLookUp[link.Source], stageLoadContext.ActorLookUp[link.Destination], link.LinkType));

        foreach(StageBcett.ActorToRailLink link in dataKeeper.GetData(stageBcett.ActorToRailLinks))
            muMap.ActorToRailLinks.Add((stageLoadContext.ActorLookUp[link.Source], stageLoadContext.RailPointLookUp[(link.Destination, link.Point)], link.LinkType));

        foreach(StageBcett.SimultaneousGroup group in dataKeeper.GetData(stageBcett.SimultaneousGroups))
        {
            var newGroup = new SimultaneousGroup();
            foreach(ulong actorHash in dataKeeper.GetData(group.Actors))
                stageLoadContext.ActorLookUp[actorHash].SimultaneousGroup = newGroup;
        }

        return (true, muMap);
    }
    
    //Todo Load stage objects with a Course loading context so all linked objects actually reference each other
    
    private readonly MuMapRef _muMapRef;
    public List<Actor> Actors = [];
    public List<Rail> Rails = [];
    public List<BGUnit> BGUnits = [];
    public List<(Actor source, Actor dest, string name)> Links = [];
    public List<(Actor source, Rail.Point dest, string name)> ActorToRailLinks = [];

    public class StageLoadContext
    {
        public Dictionary<ulong, Actor> ActorLookUp = [];
        public Dictionary<ulong, Rail> RailLookUp = [];
        public Dictionary<(ulong rail, ulong point), Rail.Point> RailPointLookUp = [];
    }
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
        stageLoadContext.ActorLookUp.Add(actorData.Hash, actor);
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
        foreach(StageRailData.Point point in dataKeeper.GetData(railData.Points))
        {
            Rail.Point newPoint = new Rail.Point{
                Dynamic = point.Dynamic,
                Translate = point.Translate,
                Control1 = point.Control1
            };
            rail.Points.Add(newPoint);
            stageLoadContext.RailPointLookUp.Add((railData.Hash, point.Hash), newPoint);
        }
        stageLoadContext.RailLookUp.Add(railData.Hash, rail);
        Rails.Add(rail);
    }
    private void AddBGUnit(CourseBgUnitData bgUnitData, IDeserializedStageDataKeeper dataKeeper)
    {
        var bGUnit = new BGUnit{
            ModelType = (BGUnit.ModelTypes)bgUnitData.ModelType,
            SkinDivision = bgUnitData.SkinDivision,
            Walls = new List<BGUnit.Wall>(bgUnitData.Walls?.Count ?? 0),
            BeltRails = new List<BGUnit.Rail>(bgUnitData.BeltRails?.Count ?? 0),
        };
        foreach(CourseBgUnitData.Wall wall in dataKeeper.GetData(bgUnitData.Walls))
        {
            BGUnit.Wall newWall = new BGUnit.Wall{
                ExternalRail = CreateBGUnitRail(dataKeeper.GetData(wall.ExternalRail), dataKeeper),
                InternalRails = new List<BGUnit.Rail>(wall.InternalRails?.Count ?? 0)
            };
            foreach (CourseBgUnitData.Rail rail in dataKeeper.GetData(wall.InternalRails))
            {
                newWall.InternalRails.Add(CreateBGUnitRail(rail, dataKeeper));
            }
        }
        foreach (CourseBgUnitData.Rail rail in dataKeeper.GetData(bgUnitData.BeltRails))
        {
            bGUnit.BeltRails.Add(CreateBGUnitRail(rail, dataKeeper));
        }
        BGUnits.Add(bGUnit);
    }

    private BGUnit.Rail CreateBGUnitRail(CourseBgUnitData.Rail bgUnitRailData, IDeserializedStageDataKeeper dataKeeper)
    {
        var rail = new BGUnit.Rail{
            Points = new List<BGUnit.Point>(bgUnitRailData.Points.Count),
            IsClosed = bgUnitRailData.IsClosed,
        };
        foreach(CourseBgUnitData.Point point in dataKeeper.GetData(bgUnitRailData.Points))
        {
            BGUnit.Point newPoint = new BGUnit.Point{
                Translate = point.Translate,
            };
            rail.Points.Add(newPoint);
        }
        return rail;
    }
}