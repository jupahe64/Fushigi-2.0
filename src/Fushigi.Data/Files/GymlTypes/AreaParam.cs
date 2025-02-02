using Fushigi.Data.BymlSerialization;
using Fushigi.Data.LevelObjects;
using BymlLibrary;
using ZstdSharp;
using Revrs;

namespace Fushigi.Data.Files.GymlTypes;

// Currently doesn't load anything, but it shouldn't be hard for it to do so
public class AreaParam : SerializableBymlObject<AreaParam>
{
    public static readonly string GymlTypeSuffix = "game__stage__AreaParam";
    public static readonly string[] DefaultSavePath = ["Stage", "AreaParam"];

    public string BgmType = null!;
    public string WonderBgmType = null!;
    public string EnvSetName = null!;
    public string EnvironmentSound = null!;
    public string EnvironmentSoundEfx = null!;
    public bool IsNotCallWaterEnvSE;
    public float WonderBgmStartOffset;
    public PropertyDict SkinParam = null!;

    protected override void Deserialize(Deserializer d)
    {
    }

    protected override void Serialize(Serializer s)
    {
    }

    public class EnvPalette : SerializableBymlObject<EnvPalette>
    {
        public float InitPaletteBaseName;
        public List<string> WonderPaletteList = null!;

        protected override void Deserialize(Deserializer d)
        {
        }

        protected override void Serialize(Serializer s)
        {
        }
    }
}