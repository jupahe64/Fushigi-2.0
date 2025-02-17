using Fushigi.Data.BymlSerialization;
using Fushigi.Data.StageObjects;

namespace Fushigi.Data.Files;

/// <summary>
/// Represents the stage content files stored as bcett.byml.zs
/// </summary>
public class StageBcett : SerializableBymlObject<StageBcett>
{
    public static readonly string DefaultSavePath = "BancMapUnit";

    public uint RootAreaHash;
    public string StageParamPath = null!;
    public List<StageActor> Actors = null!;
    public List<StageRail> Rails = null!;
    public List<CourseBgUnit> BgUnits = null!;
    public List<CourseLink> Links = null!;
    public List<CourseLink> ActorToRailLinks = null!;
    public List<SimultaneousGroup> SimultaneousGroups = null!;
    public List<string> RefStages = null!;
    
    protected override void Deserialize(ISerializationContext ctx)
    {
        ctx.Set(UINT32, ref RootAreaHash, "RootAreaHash");
        ctx.Set(STRING, ref StageParamPath, "StageParam");
        ctx.SetArray(ref Actors, "Actors", StageActor.Conversion);
        ctx.SetArray(ref Rails, "Rails", StageRail.Conversion, optional: true);
        ctx.SetArray(ref BgUnits, "BgUnits", CourseBgUnit.Conversion, optional: true);
        ctx.SetArray(ref Links, "Links", CourseLink.Conversion, optional: true);
        ctx.SetArray(ref ActorToRailLinks, "ActorToRailLinks", CourseLink.Conversion, optional: true);
        ctx.SetArray(ref SimultaneousGroups, "SimultaneousGroups", SimultaneousGroup.Conversion, optional: true);
        ctx.SetArray(ref RefStages, "RefStages", STRING, optional: true);
    }

    protected override void Serialize(ISerializationContext ctx)
    {
        ctx.Set(UINT32, ref RootAreaHash, "RootAreaHash");
        ctx.Set(STRING, ref StageParamPath, "StageParam");
        ctx.SetArray(ref Actors, "Actors", StageActor.Conversion);
        ctx.SetArray(ref Rails, "Rails", StageRail.Conversion, optional: true);
        ctx.SetArray(ref BgUnits, "BgUnits", CourseBgUnit.Conversion, optional: true);
        ctx.SetArray(ref Links, "Links", CourseLink.Conversion, optional: true);
        ctx.SetArray(ref ActorToRailLinks, "ActorToRailLinks", CourseLink.Conversion, optional: true);
        ctx.SetArray(ref SimultaneousGroups, "SimultaneousGroups", SimultaneousGroup.Conversion, optional: true);
        ctx.SetArray(ref RefStages, "RefStages", STRING, optional: true);
    }

    public class CourseLink : SerializableBymlObject<CourseLink>
    {
        public ulong Destination;
        public ulong Source;
        public ulong Point;
        public string LinkType = null!;

        protected override void Deserialize(ISerializationContext ctx)
        {
            ctx.Set(UINT64, ref Destination, "Dst");
            ctx.Set(UINT64, ref Source, "Src");
            ctx.Set(STRING, ref LinkType, "Name");
            ctx.Set(UINT64, ref Point, "Point", optional: true);
        }

        protected override void Serialize(ISerializationContext ctx)
        {
            ctx.Set(UINT64, ref Destination, "Dst");
            ctx.Set(UINT64, ref Source, "Src");
            ctx.Set(STRING, ref LinkType, "Name");
            ctx.Set(UINT64, ref Point, "Point", optional: true);
        }
    }

    public class SimultaneousGroup : SerializableBymlObject<SimultaneousGroup>
    {
        public ulong Hash;
        public List<ulong> Actors = null!;

        protected override void Deserialize(ISerializationContext ctx)
        {
            ctx.Set(UINT64, ref Hash, "Hash");
            ctx.SetArray(ref Actors, "Actors", UINT64);
        }

        protected override void Serialize(ISerializationContext ctx)
        {
            ctx.Set(UINT64, ref Hash, "Hash");
            ctx.SetArray(ref Actors, "Actors", UINT64);
        }
    }
}