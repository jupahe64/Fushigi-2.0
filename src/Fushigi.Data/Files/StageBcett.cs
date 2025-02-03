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
    
    protected override void Deserialize(Deserializer d)
    {
        d.SetUInt32(ref RootAreaHash, "RootAreaHash");
        d.SetString(ref StageParamPath, "StageParam");
        d.SetArray(ref Actors, "Actors", StageActor.Conversion);
        d.SetArray(ref Rails, "Rails", StageRail.Conversion, true);
        d.SetArray(ref BgUnits, "BgUnits", CourseBgUnit.Conversion, true);
        d.SetArray(ref Links, "Links", CourseLink.Conversion, true);
        d.SetArray(ref ActorToRailLinks, "ActorToRailLinks", CourseLink.Conversion, true);
        d.SetArray(ref SimultaneousGroups, "SimultaneousGroups", SimultaneousGroup.Conversion, true);
        d.SetArray(ref RefStages, "RefStages", d.ConvertString, true);
    }

    protected override void Serialize(Serializer s)
    {
        s.SetUInt32(ref RootAreaHash, "RootAreaHash");
        s.SetString(ref StageParamPath, "StageParam");
        s.SetArray(ref Actors, "Actors", StageActor.Conversion);
        s.SetArray(ref Rails, "Rails", StageRail.Conversion, true);
        s.SetArray(ref BgUnits, "BgUnits", CourseBgUnit.Conversion, true);
        s.SetArray(ref Links, "Links", CourseLink.Conversion, true);
        s.SetArray(ref ActorToRailLinks, "ActorToRailLinks", CourseLink.Conversion, true);
        s.SetArray(ref SimultaneousGroups, "SimultaneousGroups", SimultaneousGroup.Conversion, true);
        s.SetArray(ref RefStages, "RefStages", s.ConvertString, true);
    }

    public class CourseLink : SerializableBymlObject<CourseLink>
    {
        public ulong Destination;
        public ulong Source;
        public ulong Point;
        public string LinkType = null!;

        protected override void Deserialize(Deserializer d)
        {
            d.SetUInt64(ref Destination, "Dst");
            d.SetUInt64(ref Source, "Src");
            d.SetString(ref LinkType, "Name");
            d.SetUInt64(ref Point, "Point", true);
        }

        protected override void Serialize(Serializer s)
        {
            s.SetUInt64(ref Destination, "Dst");
            s.SetUInt64(ref Source, "Src");
            s.SetString(ref LinkType, "Name");
            s.SetUInt64(ref Point, "Point", true);
        }
    }

    public class SimultaneousGroup : SerializableBymlObject<SimultaneousGroup>
    {
        public ulong Hash;
        public List<ulong> Actors = null!;

        protected override void Deserialize(Deserializer d)
        {
            d.SetUInt64(ref Hash, "Hash");
            d.SetArray(ref Actors, "Actors", d.ConvertUInt64);
        }

        protected override void Serialize(Serializer s)
        {
            s.SetUInt64(ref Hash, "Hash");
            s.SetArray(ref Actors, "Actors", s.ConvertUInt64);
        }
    }
}