using System.Numerics;
using Fushigi.Data.BymlSerialization;

namespace Fushigi.Data.StageObjects;

public class StageActor : SerializableBymlObject<StageActor>
{
    public uint AreaHash;
    public ulong Hash;
    public string Gyaml = null!;
    public string Name = null!;
    public string Layer = null!;
    public PropertyDict Dynamic = PropertyDict.Empty;
    public Vector3 Rotate;
    public Vector3 Scale;
    public Vector3 Translate;

    protected override void Deserialize<TContext>(TContext ctx)
    {
        ctx.Set(UINT32, ref AreaHash, "AreaHash");
        ctx.Set(UINT64, ref Hash, "Hash");
        ctx.Set(STRING, ref Gyaml, "Gyaml");
        ctx.Set(STRING, ref Name, "Name");
        ctx.Set(STRING, ref Layer, "Layer");
        ctx.Set(PROPERTY_DICT, ref Dynamic, "Dynamic", optional: true);
        ctx.Set(FLOAT3, ref Rotate, "Rotate");
        ctx.Set(FLOAT3, ref Scale, "Scale");
        ctx.Set(FLOAT3, ref Translate, "Translate");
    }

    protected override void Serialize<TContext>(TContext ctx)
    {
        ctx.Set(UINT32, ref AreaHash, "AreaHash");
        ctx.Set(UINT64, ref Hash, "Hash");
        ctx.Set(STRING, ref Gyaml, "Gyaml");
        ctx.Set(STRING, ref Name, "Name");
        ctx.Set(STRING, ref Layer, "Layer");
        ctx.Set(PROPERTY_DICT, ref Dynamic, "Dynamic", optional: true);
        ctx.Set(FLOAT3, ref Rotate, "Rotate");
        ctx.Set(FLOAT3, ref Scale, "Scale");
        ctx.Set(FLOAT3, ref Translate, "Translate");
    }
}
