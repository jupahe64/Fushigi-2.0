using System.Numerics;
using Fushigi.Data.BymlSerialization;

namespace Fushigi.Data.StageObjects;

public class StageRail : SerializableBymlObject<StageRail>
{
    public List<RailPoint> Points = null!;
    public uint AreaHash;
    public ulong Hash;
    public string Gyaml = null!;
    public bool IsClosed;
    public PropertyDict Dynamic = PropertyDict.Empty;

    protected override void Deserialize<TContext>(TContext ctx)
    {
        ctx.SetArray(ref Points, "Points", RailPoint.Conversion);
        ctx.Set(UINT32, ref AreaHash, "AreaHash");
        ctx.Set(UINT64, ref Hash, "Hash");
        ctx.Set(STRING, ref Gyaml, "Gyaml");
        ctx.Set(BOOL, ref IsClosed, "IsClosed");
        ctx.Set(PROPERTY_DICT, ref Dynamic, "Dynamic", optional: true);
    }

    protected override void Serialize<TContext>(TContext ctx)
    {
        ctx.SetArray(ref Points, "Points", RailPoint.Conversion);
        ctx.Set(UINT32, ref AreaHash, "AreaHash");
        ctx.Set(UINT64, ref Hash, "Hash");
        ctx.Set(STRING, ref Gyaml, "Gyaml");
        ctx.Set(BOOL, ref IsClosed, "IsClosed");
        ctx.Set(PROPERTY_DICT, ref Dynamic, "Dynamic", optional: true);
    }
}
public class RailPoint : SerializableBymlObject<RailPoint>
{
    public ulong Hash;
    public PropertyDict Dynamic = PropertyDict.Empty;
    public Vector3 Translate;
    public Vector3? CurveControl;

    protected override void Deserialize<TContext>(TContext ctx)
    {
        ctx.Set(UINT64, ref Hash, "Hash", optional: true);
        ctx.Set(PROPERTY_DICT, ref Dynamic, "Dynamic", optional: true);
        ctx.Set(FLOAT3, ref Translate, "Translate");
        ctx.Set(FLOAT3, ref CurveControl, "Control1", optional: true);
    }

    protected override void Serialize<TContext>(TContext ctx)
    {
        ctx.Set(UINT64, ref Hash, "Hash", optional: true);
        ctx.Set(PROPERTY_DICT, ref Dynamic, "Dynamic", optional: true);
        ctx.Set(FLOAT3, ref Translate, "Translate");
        ctx.Set(FLOAT3, ref CurveControl, "Control1", optional: true);
    }
}