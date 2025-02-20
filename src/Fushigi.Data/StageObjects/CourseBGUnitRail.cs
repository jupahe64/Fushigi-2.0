using System.Numerics;
using Fushigi.Data.BymlSerialization;
using static Fushigi.Data.BymlSerialization.BymlObjectConversions;

namespace Fushigi.Data.StageObjects;

public struct CourseBgUnitData()
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
    public required ListSegment<Wall>? Walls;
    public required ListSegment<Rail>? BeltRails;

    private void Serialization<TContext>(TContext ctx)
        where TContext : ISerializationContext
    {
        ctx.Set(MODEL_TYPE, ref ModelType, "ModelType");
        ctx.Set(INT32, ref SkinDivision, "SkinDivision");
    }
    
    internal static CourseBgUnitData Deserialize(Deserializer d, StageObjectsDeserializer sod)
    {
        var data = new CourseBgUnitData
        {
            Walls = sod.DeserializeArray(d, "Walls", Wall.Deserialize, optional: true),
            BeltRails = sod.DeserializeArray(d, "BeltRails", Rail.Deserialize, optional: true),
        }; 
        data.Serialization(d); return data;
    }

    public struct Wall()
    {
        public required ListRef<Rail> ExternalRail;
        public required ListSegment<Rail>? InternalRails;

        internal static Wall Deserialize(Deserializer d, StageObjectsDeserializer sod)
        {
            var data = new Wall
            {
                ExternalRail = sod.Deserialize(d, "ExternalRail", Rail.Deserialize)!.Value,
                InternalRails = sod.DeserializeArray(d, "InternalRails", Rail.Deserialize, optional: true),
            }; 
            return data;
        }
    }
    public struct Rail()
    {
        public required ListSegment<Point> Points;
        public bool IsClosed;

        private void Serialization<TContext>(TContext ctx)
            where TContext : ISerializationContext
        {
            ctx.Set(BOOL, ref IsClosed, "IsClosed");
        }
        
        internal static Rail Deserialize(Deserializer d, StageObjectsDeserializer sod)
        {
            var data = new Rail
            {
                Points = sod.DeserializeArray(d, "Points", Point.Deserialize)!.Value,
            };
            data.Serialization(d); return data;
        }
    }
    
    public struct Point()
    {
        public Vector3 Translate;

        private void Serialization<TContext>(TContext ctx)
            where TContext : ISerializationContext
        {
            ctx.Set(FLOAT3, ref Translate, "Translate");
        }
        
        internal static Point Deserialize(Deserializer deserializer, StageObjectsDeserializer sod)
        {
            var data = new Point(); data.Serialization(deserializer); return data;
        }
    }
}