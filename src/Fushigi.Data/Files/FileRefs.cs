using Fushigi.Data.Files.GymlTypes;

namespace Fushigi.Data.Files;

public struct MuMapRef : IFileRef
{
    public static (string inProduction, string shipped) Suffix => (".mumap", ".bcett.byml.zs");
    public string ValidatedRefPath { get; set; }
}

public struct StaticCompoundBodySourceParamRef : IFileRef
{
    public static (string inProduction, string shipped) Suffix 
        => (".phive__StaticCompoundBodySourceParam.gyml", ".Nin_NX_NVN.bphsc.zs");
    public string ValidatedRefPath { get; set; }
}

public struct GymlRef<TGymlFile> : IFileRef
    where TGymlFile : GymlFile<TGymlFile>, IGymlType, new()
{
    // ReSharper disable once StaticMemberInGenericType
    public static (string inProduction, string shipped) Suffix { get; } = (
        $".{TGymlFile.GymlTypeSuffix}.gyml", 
        $".{TGymlFile.GymlTypeSuffix}.bgyml"
    );

    public string ValidatedRefPath { get; set; }
}