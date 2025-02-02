using Fushigi.Data.BymlSerialization;
using Fushigi.Data;
using BymlLibrary;
using System.Numerics;

namespace Fushigi.Data.LevelObjects;

public class CourseBgUnit : SerializableBymlObject<CourseBgUnit>
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
    public List<Wall> Walls = null!;
    public List<BgUnitRail> BeltRails = null!;

    protected override void Deserialize(Deserializer d)
    {
        int type = 0;
        d.SetInt32(ref type, "ModelType");
        ModelType = (ModelTypes)type;
        d.SetInt32(ref SkinDivision, "SkinDivision");
        d.SetArray(ref Walls, "Walls", Wall.Conversion, true);
        d.SetArray(ref BeltRails, "BeltRails", BgUnitRail.Conversion, true);
    }

    protected override void Serialize(Serializer s)
    {
        int type = (int)ModelType;
        s.SetInt32(ref type, "ModelType");
        ModelType = (ModelTypes)type;
        s.SetInt32(ref SkinDivision, "SkinDivision");
        s.SetArray(ref Walls, "Walls", Wall.Conversion, true);
        s.SetArray(ref BeltRails, "BeltRails", BgUnitRail.Conversion, true);
    }

    public class Wall : SerializableBymlObject<Wall>
    {
        public BgUnitRail ExternalRail = null!;
        public List<BgUnitRail> InternalRails = null!;
        protected override void Deserialize(Deserializer d)
        {
            d.SetObject(ref ExternalRail, "ExternalRail");
            d.SetArray(ref InternalRails, "InternalRails", BgUnitRail.Conversion, true);
        }
        protected override void Serialize(Serializer s)
        {
            s.SetObject(ref ExternalRail, "ExternalRail");
            s.SetArray(ref InternalRails, "InternalRails", BgUnitRail.Conversion, true);
        }
    }
    public class BgUnitRail : SerializableBymlObject<BgUnitRail>
    {
        public List<RailPoint> Points = null!;
        public bool IsClosed;

        protected override void Deserialize(Deserializer d)
        {
            d.SetArray(ref Points, "Points", RailPoint.Conversion);
            d.SetBool(ref IsClosed, "IsClosed");
        }
        protected override void Serialize(Serializer s)
        {
            s.SetArray(ref Points, "Points", RailPoint.Conversion);
            s.SetBool(ref IsClosed, "IsClosed");
        }
    }
}