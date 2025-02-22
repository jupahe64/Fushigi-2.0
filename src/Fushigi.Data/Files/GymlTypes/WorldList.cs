using BymlLibrary;
using Fushigi.Data.BymlSerialization;

namespace Fushigi.Data.Files.GymlTypes;

/// <summary>
/// Serializable game.stage.WorldList
/// </summary>
public class WorldList : GymlFile<WorldList>
{
    // Nintendo decided it'd be a good idea to have empty entries in WorldList
    // so yeah...
    public struct GymlRefOrEmpty
    {
        public static readonly GymlRefOrEmpty Empty = new();
        public static GymlRefOrEmpty Create(GymlRef gymlRef) => new() {GymlRef = gymlRef};
        public static implicit operator GymlRefOrEmpty(GymlRef gymlRef) => new() {GymlRef = gymlRef};
        public GymlRef? GymlRef { get; private init; }
        
        // ReSharper disable once MemberHidesStaticFromOuterClass
        public static BymlConversion<GymlRefOrEmpty> Conversion => 
            new(BymlNodeType.String, Deserialize, Serialize);
        
        private static Byml Serialize(GymlRefOrEmpty value)
        {
            return value.GymlRef is null ?
                new Byml("") : new Byml(value.GymlRef.Value.ValidatedRefPath);
        }

        private static GymlRefOrEmpty Deserialize(Deserializer deserializer)
        {
            var byml = deserializer.GetNode();
            string value = byml.GetString();
            if (value == string.Empty)
                return new GymlRefOrEmpty();
            
            if (!FileRefConversion.IsValid<GymlRef>(value))
                deserializer.ReportInvalidRefPath();
        
            return new GymlRefOrEmpty { GymlRef = new GymlRef {ValidatedRefPath = value} };
        }
    }
    
    public static readonly string GymlTypeSuffix = "game__stage__WorldList";
    public static readonly string[] DefaultSavePath = ["Stage", "WorldList"];
    
    public List<GymlRefOrEmpty> WorldMapStagePath = null!;
    
    #region De/Serialization

    protected override void Serialization<TContext>(TContext ctx)
    {
        base.Serialization(ctx);
        ctx.SetArray(ref WorldMapStagePath, "WorldMapStagePath", GymlRefOrEmpty.Conversion);
    }
    #endregion
}