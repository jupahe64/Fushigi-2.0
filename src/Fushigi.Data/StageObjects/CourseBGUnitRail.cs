using Fushigi.Data.BymlSerialization;

namespace Fushigi.Data.StageObjects;

public class CourseBgUnit : SerializableBymlObject<CourseBgUnit>
{
    private static BymlConversion<ModelTypes> MODEL_TYPE = SpecialConversions.GetEnumConversion<ModelTypes>();
    public enum ModelTypes
    {
        Solid, 
        SemiSolid, 
        NoCollision, 
        Bridge
    }
    public ModelTypes ModelType;
    public int SkinDivision;
    public List<Wall> Walls = null!;
    public List<BgUnitRail> BeltRails = null!;

    protected override void Serialization<TContext>(TContext ctx)
    {
        ctx.Set(MODEL_TYPE, ref ModelType, "ModelType");
        ctx.Set(INT32, ref SkinDivision, "SkinDivision");
        ctx.SetArray(ref Walls, "Walls", Wall.Conversion, true);
        ctx.SetArray(ref BeltRails, "BeltRails", BgUnitRail.Conversion, true);
    }

    public class Wall : SerializableBymlObject<Wall>
    {
        public BgUnitRail ExternalRail = null!;
        public List<BgUnitRail> InternalRails = null!;

        protected override void Serialization<TContext>(TContext ctx)
        {
            ctx.SetObject(ref ExternalRail, "ExternalRail");
            ctx.SetArray(ref InternalRails, "InternalRails", BgUnitRail.Conversion, true);
        }
    }
    public class BgUnitRail : SerializableBymlObject<BgUnitRail>
    {
        public List<RailPoint> Points = null!;
        public bool IsClosed;

        protected override void Serialization<TContext>(TContext ctx)
        {
            ctx.SetArray(ref Points, "Points", RailPoint.Conversion);
            ctx.Set(BOOL, ref IsClosed, "IsClosed");
        }
    }
}