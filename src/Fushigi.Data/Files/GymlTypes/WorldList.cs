using BymlLibrary;
using Fushigi.Data.BymlSerialization;

namespace Fushigi.Data.Files.GymlTypes;

/// <summary>
/// Serializable game.stage.WorldList
/// </summary>
public class WorldList : GymlFile<WorldList>, IGymlType
{
    public static string GymlTypeSuffix => "game__stage__WorldList";
    public static readonly string[] DefaultSavePath = ["Stage", "WorldList"];
    
    // Nintendo decided it'd be a good idea to have empty entries in WorldList
    // so yeah...
    public struct GymlRefOrEmpty<TGymlFile>
        where TGymlFile : GymlFile<TGymlFile>, IGymlType, new()
    {
        public static readonly GymlRefOrEmpty<TGymlFile> Empty = new();
        public static GymlRefOrEmpty<TGymlFile> Create(GymlRef<TGymlFile> gymlRef) => new() {GymlRef = gymlRef};
        public static implicit operator GymlRefOrEmpty<TGymlFile>(GymlRef<TGymlFile> gymlRef) => new() {GymlRef = gymlRef};
        public GymlRef<TGymlFile>? GymlRef { get; private init; }
        
        // ReSharper disable once MemberHidesStaticFromOuterClass
        public static BymlConversion<GymlRefOrEmpty<TGymlFile>> Conversion => 
            new(BymlNodeType.String, Deserialize, Serialize);
        
        private static Byml Serialize(GymlRefOrEmpty<TGymlFile> value)
        {
            return value.GymlRef is null ?
                new Byml("") : new Byml(value.GymlRef.Value.ValidatedRefPath);
        }

        private static GymlRefOrEmpty<TGymlFile> Deserialize(Deserializer deserializer)
        {
            var byml = deserializer.GetNode();
            string value = byml.GetString();
            if (value == string.Empty)
                return new GymlRefOrEmpty<TGymlFile>();
            
                deserializer.ReportInvalidRefPath();
            if (!FileRefConversion.IsValid<GymlRef<TGymlFile>>(value))
        
            return new GymlRefOrEmpty<TGymlFile> { GymlRef = new GymlRef<TGymlFile> {ValidatedRefPath = value} };
        }
    }
    
    public List<GymlRefOrEmpty<StageParam>> WorldMapStagePath = null!;
    
    #region De/Serialization

    protected override void Serialization<TContext>(TContext ctx)
    {
        base.Serialization(ctx);
        ctx.SetArray(ref WorldMapStagePath, "WorldMapStagePath", GymlRefOrEmpty<StageParam>.Conversion);
    }
    #endregion
}