using BymlLibrary;
using Fushigi.Data.BymlSerialization;

namespace Fushigi.Data.Files;

public static class FileRefConversion
{
    public record FileRefPathFormat(string Prefix, string Suffix);
    public static BymlConversion<T> For<T>() where T : struct, IFileRef 
        => new(BymlNodeType.String, Deserialize<T>, Serialize);

    public static FileRefPathFormat GetFileRefPathFormat(string suffix)
    {
        return new FileRefPathFormat(CommonPrefix, suffix);
    }

    private static Byml Serialize<T>(T value)
        where T : struct, IFileRef
    {
        return new Byml(value.ValidatedRefPath);
    }

    private static T Deserialize<T>(Deserializer deserializer)
        where T : struct, IFileRef
    {
        string value = deserializer.GetNode().GetString();
        if (!IsValid<T>(value))
            deserializer.ReportInvalidRefPath(GetFileRefPathFormat(T.Suffix.inProduction));
        
        return new T { ValidatedRefPath = value };
    }

    private const string CommonPrefix = "Work/";

    private static readonly List<(string inProduction, string shipped)> s_redirects =
    [
        ("MapUnit/Map/", "BancMapUnit/")
    ];

    public static bool IsValid<T>(string fileRefPath) where T : struct, IFileRef
        => fileRefPath.StartsWith(CommonPrefix) && fileRefPath.EndsWith(T.Suffix.inProduction);
    
    
    public static string[] GetRomFSFilePath<T>(T fileRef)
        where T : struct, IFileRef
    {
        if (!IsValid<T>(fileRef.ValidatedRefPath))
        {
            throw new ArgumentException($"{nameof(fileRef.ValidatedRefPath)} is not valid", 
                nameof(fileRef));
        }

        string middlePart = fileRef.ValidatedRefPath[CommonPrefix.Length..^T.Suffix.inProduction.Length];

        foreach ((string inProduction, string shipped) in s_redirects)
        {
            if (!middlePart.StartsWith(inProduction))
                continue;
            
            middlePart = shipped + middlePart[inProduction.Length..];
            break;
        }
        string[] romFSPath = middlePart.Split('/');
        romFSPath[^1] += T.Suffix.shipped;
        return romFSPath;
    }

    public static T Parse<T>(string validFileRefPath)
        where T : struct, IFileRef
    {
        if (!IsValid<T>(validFileRefPath))
            throw new ArgumentException($"{nameof(validFileRefPath)} is not valid", 
                nameof(validFileRefPath));
        
        return new T { ValidatedRefPath = validFileRefPath };
    }
}