using BymlLibrary;
using Fushigi.Data.BymlSerialization;
using Fushigi.Data.StageObjects;
using static Fushigi.Data.BymlSerialization.BymlObjectConversions;

namespace Fushigi.Data.Files;

/// <summary>
/// Represents the stage content files stored as bcett.byml.zs
/// </summary>
public static class StageBcett
{
    public static readonly string DefaultSavePath = "BancMapUnit";
    
    public struct Content()
    {
        public required ListSegment<StageActorData> Actors;
        public required ListSegment<StageRailData>? Rails;
        public required ListSegment<CourseBgUnitData>? BgUnits;
        public required ListSegment<Link>? Links;
        public required ListSegment<ActorToRailLink>? ActorToRailLinks;
        public required ListSegment<SimultaneousGroup>? SimultaneousGroups;
        public required ListSegment<string>? RefStages;
        
        public uint RootAreaHash;
        public string StageParamPath = null!;
        
        internal void Serialization<TContext>(TContext ctx)
            where TContext : ISerializationContext
        {
            ctx.Set(UINT32, ref RootAreaHash, "RootAreaHash");
            ctx.Set(STRING, ref StageParamPath, "StageParam");
        }
    }
    
    public static async Task<(bool success, (Content content, IDeserializedStageDataKeeper dataKeeper))> 
        DeserializeFrom(Byml byml, 
        IBymlDeserializeErrorHandler errorHandler) =>
        await Deserializer.Deserialize(byml, Deserialize, errorHandler);

    private static (Content content, IDeserializedStageDataKeeper dataKeeper) Deserialize(Deserializer d)
    {
        var sod = new StageObjectsDeserializer();
        
        var content = new Content
        {
            Actors = sod.DeserializeArray(d, "Actors", StageActorData.Deserialize)!.Value,
            Rails = sod.DeserializeArray(d, "Rails", StageRailData.Deserialize, optional: true),
            BgUnits = sod.DeserializeArray(d, "BgUnits", CourseBgUnitData.Deserialize, optional: true),
            Links = sod.DeserializeArray(d, "Links", Link.Deserialize, optional: true),
            ActorToRailLinks = sod.DeserializeArray(d, "ActorToRailLinks", ActorToRailLink.Deserialize, optional: true),
            SimultaneousGroups = sod.DeserializeArray(d, "SimultaneousGroups", SimultaneousGroup.Deserialize, optional: true),
            RefStages = sod.DeserializeArray(d, "RefStages", 
                (sd,_)=>STRING.Deserialize(sd), STRING.RequiredNodeType, optional: true),
        };
        content.Serialization(d);
        return (content, sod);
    }
    
    public struct Link()
    {
        public ulong Destination;
        public ulong Source;
        public string LinkType = null!;

        private void Serialization<TContext>(TContext ctx)
            where TContext : ISerializationContext
        {
            ctx.Set(UINT64, ref Destination, "Dst");
            ctx.Set(UINT64, ref Source, "Src");
            ctx.Set(STRING, ref LinkType, "Name");
        }
        
        internal static Link Deserialize(Deserializer d, StageObjectsDeserializer sod)
        {
            var data = new Link(); data.Serialization(d); return data;
        }
    }
    
    public struct ActorToRailLink()
    {
        public ulong Destination;
        public ulong Source;
        public ulong Point;
        public string LinkType = null!;

        private void Serialization<TContext>(TContext ctx)
            where TContext : ISerializationContext
        {
            ctx.Set(UINT64, ref Destination, "Dst");
            ctx.Set(UINT64, ref Source, "Src");
            ctx.Set(STRING, ref LinkType, "Name");
            ctx.Set(UINT64, ref Point, "Point");
        }
        
        internal static ActorToRailLink Deserialize(Deserializer d, StageObjectsDeserializer sod)
        {
            var data = new ActorToRailLink(); data.Serialization(d); return data;
        }
    }

    public struct SimultaneousGroup
    {
        public ulong Hash;
        public required ListSegment<ulong> Actors;

        private void Serialization<TContext>(TContext ctx)
            where TContext : ISerializationContext
        {
            ctx.Set(UINT64, ref Hash, "Hash");
        }
        
        internal static SimultaneousGroup Deserialize(Deserializer d, StageObjectsDeserializer sod)
        {
            var data = new SimultaneousGroup()
            {
                Actors = sod.DeserializeArray(d, "Actors", 
                    (sd,_)=>UINT64.Deserialize(sd), UINT64.RequiredNodeType)!.Value,
            };
            data.Serialization(d);
            return data;
        }
    }
}