// ReSharper disable InvertIf

namespace Fushigi.Data;

internal static class RomFSLoading
{
    public static async Task<bool> EnsureValid(
        string baseGameDirectory, string? modDirectory,
        Func<Task> onBaseGameAndModPathsIdentical,
        Func<RootDirectoryNotFoundErrorInfo, Task> onRootDirectoryNotFound,
        Func<MissingSubDirectoryErrorInfo, Task> onMissingSubDirectory)
    {
        //Check if root directory for baseGame exists
        if (modDirectory != null &&
            Path.GetFullPath(baseGameDirectory) == Path.GetFullPath(modDirectory))
        {
            await onBaseGameAndModPathsIdentical();
            return false;
        }
        
        if (!Directory.Exists(baseGameDirectory))
        {
            await onRootDirectoryNotFound(new RootDirectoryNotFoundErrorInfo(baseGameDirectory));
            return false;
        }

        //Check if required directories exist
        foreach (string subDirectory in RomFS.RequiredDirectories)
        {
            if (!Directory.Exists(Path.Combine(baseGameDirectory, subDirectory)))
            {
                await onMissingSubDirectory(new MissingSubDirectoryErrorInfo(baseGameDirectory, subDirectory));
                return false;
            } 
        }
        
        //Check if root directory for mod exists
        if (modDirectory != null && !Directory.Exists(modDirectory))
        {
            await onRootDirectoryNotFound(new RootDirectoryNotFoundErrorInfo(modDirectory));
            return false;
        }
        return true;
    }

    public static async Task<string?> ResolveSystemFileLocation(string baseGameDirectory, string? modDirectory,
        RomFS.SystemFile kind,
        string[] fileLocation,
        Func<MissingSystemFileErrorInfo, Task>? onMissingSystemFile)
    {
        string? filePath = await ResolveSystemFileLocation(baseGameDirectory, kind, fileLocation, 
            onMissingSystemFile);
            
        if (filePath == null)
            return null;

        if (modDirectory != null)
        {
            string? modFilePath = await ResolveSystemFileLocation(modDirectory, kind, fileLocation, 
                //a missing system file in the mod directory is not an error so don't report it
                onMissingSystemFile: null);
            
            if (modFilePath != null)
                filePath = modFilePath;
        }
        return filePath;
    }
    
    public static async Task<string?> ResolveSystemFileLocation(string rootDirectory, RomFS.SystemFile kind,
        string[] fileLocation,
        Func<MissingSystemFileErrorInfo, Task>? onMissingSystemFile)
    {
        var directoryLocation = fileLocation.AsSpan(..^1);
        string fileNamePattern = fileLocation[^1];
        
        //check if directory exists
        string directory = Path.Combine([rootDirectory, ..directoryLocation]);
        if (!Directory.Exists(directory))
        {
            if (onMissingSystemFile != null)
            {
                await onMissingSystemFile(new MissingSystemFileErrorInfo(
                    directory, fileNamePattern, 
                    kind, DirectoryExists: false));
            }
            return await Task.FromResult<string?>(null);
        }
        string? filePath = Directory.EnumerateFiles(directory, fileNamePattern)
            .FirstOrDefault();

        //check if file exists
        if (filePath == null)
        {
            if (onMissingSystemFile != null)
            {
                await onMissingSystemFile(new MissingSystemFileErrorInfo(
                    directory, fileNamePattern, 
                    kind, DirectoryExists: true));
            }
            return await Task.FromResult<string?>(null);
        }
        
        return filePath;
    }
}