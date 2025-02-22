namespace Fushigi.Data.Files;

public struct MuMapRef : IFileRef
{
    public static (string inProduction, string shipped) Suffix => (".mumap", ".bcett.byml.zs");
    public string ValidatedRefPath { get; set; }
}

public struct GymlRef : IFileRef
{
    public static (string inProduction, string shipped) Suffix => (".gyml", ".bgyml");
    public string ValidatedRefPath { get; set; }
}