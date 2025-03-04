using System.Numerics;
using Fushigi.Data;
using Fushigi.Data.Files;
using Fushigi.Data.Files.GymlTypes;

namespace Fushigi.Logic.Stages.Objects;

public sealed class BGUnit
{
    public enum ModelTypes
    {
        Solid, 
        SemiSolid, 
        NoCollision, 
        Bridge
    }
    public ModelTypes ModelType;
    public int SkinDivision;
    public required List<Wall>? Walls;
    public required List<Rail>? BeltRails;

    public class Wall()
    {
        public required Rail ExternalRail;
        public required List<Rail>? InternalRails;
    }
    public class Rail()
    {
        public required List<Point> Points;
        public bool IsClosed;
    }
    
    public class Point()
    {
        public Vector3 Translate;
    }
}