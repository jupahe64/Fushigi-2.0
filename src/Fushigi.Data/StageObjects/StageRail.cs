using System.Numerics;
using Fushigi.Data.BymlSerialization;
using static Fushigi.Data.BymlSerialization.BymlObjectConversions;

namespace Fushigi.Data.StageObjects;

public struct StageRailData()
{
    public required ListSegment<Point> Points;
    public uint AreaHash;
    public ulong Hash;
    public string Gyaml = null!;
    public bool IsClosed;
    public PropertyDict Dynamic = PropertyDict.Empty;

    private void Serialization<TContext>(TContext ctx)
        where TContext : ISerializationContext
    {
        ctx.Set(UINT32, ref AreaHash, "AreaHash");
        ctx.Set(UINT64, ref Hash, "Hash");
        ctx.Set(STRING, ref Gyaml, "Gyaml");
        ctx.Set(BOOL, ref IsClosed, "IsClosed");
        ctx.Set(PROPERTY_DICT, ref Dynamic, "Dynamic", optional: true);
    }
    
    internal static StageRailData Deserialize(Deserializer d, StageObjectsDeserializer sod)
    {
        var data = new StageRailData
        {
            Points = sod.DeserializeArray(d, "Points", Point.Deserialize)!.Value,
        };
        data.Serialization(d); return data;
    }
    
    public struct Point()
    {
        public ulong Hash;
        public PropertyDict Dynamic = PropertyDict.Empty;
        public Vector3 Translate;
        public Vector3? Control1;

        private void Serialization<TContext>(TContext ctx)
            where TContext : ISerializationContext
        {
            ctx.Set(UINT64, ref Hash, "Hash", optional: true);
            ctx.Set(PROPERTY_DICT, ref Dynamic, "Dynamic", optional: true);
            ctx.Set(FLOAT3, ref Translate, "Translate");
            ctx.Set(FLOAT3, ref Control1, "Control1", optional: true);
        }
        
        internal static Point Deserialize(Deserializer d, StageObjectsDeserializer sod)
        {
            var data = new Point();
            data.Serialization(d);
            return data;
        }
    }
}