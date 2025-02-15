﻿namespace Fushigi.Data.Files;

public record InvalidFileRefPathErrorInfo(string FileRefPath, string ExpectedSuffix);

public interface IFileRefPathErrorHandler
{
    Task OnInvalidFileRefPath(InvalidFileRefPathErrorInfo info);
}

public static class FileRefPathConversion
{
    public const string CommonPrefix = "Work/";

    public static List<(string inProduction, string shipped)> Redirects =
    [
        ("MapUnit/Map/", "BancMapUnit/")
    ];
    
    public static async Task<(bool success, string[]? romFSPath)> GetRomFSFilePath(
        string fileRefString, 
        (string inProduction, string shipped) suffix,
        IFileRefPathErrorHandler errorHandler)
    {
        if (!fileRefString.StartsWith(CommonPrefix) || !fileRefString.EndsWith(suffix.inProduction))
        {
            await errorHandler.OnInvalidFileRefPath(
                new InvalidFileRefPathErrorInfo(fileRefString, suffix.inProduction)
            );
            return (false, null);
        }
        
        string middlePart = fileRefString[CommonPrefix.Length..^suffix.inProduction.Length];

        foreach ((string inProduction, string shipped) in Redirects)
        {
            if (!middlePart.StartsWith(inProduction))
                continue;
            
            middlePart = shipped + middlePart[inProduction.Length..];
            break;
        }
        string[] romFSPath = middlePart.Split('/');
        romFSPath[^1] += suffix.shipped;
        
        return (true, romFSPath);
    }
}