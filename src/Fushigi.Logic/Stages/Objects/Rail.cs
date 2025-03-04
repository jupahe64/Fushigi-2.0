using System.Numerics;
using Fushigi.Data;
using Fushigi.Data.Files;
using Fushigi.Data.Files.GymlTypes;
using Fushigi.Data.StageObjects;

namespace Fushigi.Logic.Stages.Objects;

public sealed class Rail
{
    public required List<Point> Points;
    public string Gyaml = null!;
    public bool IsClosed;
    public PropertyDict Dynamic = PropertyDict.Empty;
    
    public class Point()
    {
        public PropertyDict Dynamic = PropertyDict.Empty;
        public Vector3 Translate;
        public Vector3? Control1;
    }
}