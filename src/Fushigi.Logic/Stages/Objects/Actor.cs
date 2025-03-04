using System.Numerics;
using Fushigi.Data;
using Fushigi.Data.Files;
using Fushigi.Data.Files.GymlTypes;

namespace Fushigi.Logic.Stages.Objects;

public sealed class Actor
{
    public string Gyaml = null!;
    public string Name = null!;
    public string Layer = null!;
    public PropertyDict Dynamic = PropertyDict.Empty;
    public Vector3 Rotate;
    public Vector3 Scale;
    public Vector3 Translate;
    public MuMap.SimultaneousGroup? SimultaneousGroup;
}