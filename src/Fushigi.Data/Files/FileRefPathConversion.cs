namespace Fushigi.Data.Files;

public record InvalidFileRefPathErrorInfo(string FileRefPath, string ExpectedSuffix);

public static class FileRefPathConversion
{
    public const string CommonPrefix = "Work/";
    
    public static async Task<(bool success, string[]? romFSPath)> GetRomFSFilePath(
        string fileRefString, 
        (string inProduction, string shipped) suffix,
        Func<InvalidFileRefPathErrorInfo, Task> onInvalidFileRefPath)
    {
        if (!fileRefString.StartsWith(CommonPrefix) || !fileRefString.EndsWith(suffix.inProduction))
        {
            await onInvalidFileRefPath(new InvalidFileRefPathErrorInfo(fileRefString, suffix.inProduction));
            return (false, null);
        }
        
        string middlePart = fileRefString[CommonPrefix.Length..^suffix.inProduction.Length];
        string[] romFSPath = middlePart.Split('/');
        romFSPath[^1] += suffix.shipped;
        
        return (true, romFSPath);
    }
}