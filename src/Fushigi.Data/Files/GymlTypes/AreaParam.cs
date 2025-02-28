using Fushigi.Data.BymlSerialization;

namespace Fushigi.Data.Files.GymlTypes;

// Currently doesn't load anything, but it shouldn't be hard for it to do so
public class AreaParam : GymlFile<AreaParam>, IGymlType
{
    protected override AreaParam This => this;
    public static string GymlTypeSuffix => "game__stage__AreaParam";
    public static readonly string[] DefaultSavePath = ["Stage", "AreaParam"];

    public INHERITED< string       > BgmType;
    public INHERITED< string       > WonderBgmType;
    public INHERITED< string       > EnvSetName;
    public INHERITED< string       > EnvironmentSound;
    public INHERITED< string       > EnvironmentSoundEfx;
    public INHERITED< bool         > IsNotCallWaterEnvSE;
    public INHERITED< float        > WonderBgmStartOffset;
    public INHERITED< PropertyDict > SkinParam;
    
    public static ref INHERITED< string       > BgmTypeProp(AreaParam self) => ref self.BgmType;
    public static ref INHERITED< string       > WonderBgmTypeProp(AreaParam self) => ref self.WonderBgmType;
    public static ref INHERITED< string       > EnvSetNameProp(AreaParam self) => ref self.EnvSetName;
    public static ref INHERITED< string       > EnvironmentSoundProp(AreaParam self) => ref self.EnvironmentSound;
    public static ref INHERITED< string       > EnvironmentSoundEfxProp(AreaParam self) => ref self.EnvironmentSoundEfx;
    public static ref INHERITED< bool         > IsNotCallWaterEnvSEProp(AreaParam self) => ref self.IsNotCallWaterEnvSE;
    public static ref INHERITED< float        > WonderBgmStartOffsetProp(AreaParam self) => ref self.WonderBgmStartOffset;
    public static ref INHERITED< PropertyDict > SkinParamProp(AreaParam self) => ref self.SkinParam;

    protected override void Serialization<TContext>(TContext ctx)
    {
        base.Serialization(ctx);
    }

    public class EnvPalette : SerializableBymlObject<EnvPalette>
    {
        public INHERITED< float        > InitPaletteBaseName;
        public INHERITED< List<string> > WonderPaletteList;

        protected override void Serialization<TContext>(TContext ctx)
        {
        }
    }
}