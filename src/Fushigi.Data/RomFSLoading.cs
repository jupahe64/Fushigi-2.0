// ReSharper disable InvertIf

namespace Fushigi.Data;

internal static class RomFSLoading
{
    public static async Task<bool> EnsureValid(
        string baseGameDirectory, string? modDirectory,
        IRomFSLoadingErrorHandler errorHandler)
    {
        //Check if root directory for baseGame exists
        if (modDirectory != null &&
            Path.GetFullPath(baseGameDirectory) == Path.GetFullPath(modDirectory))
        {
            await errorHandler.OnBaseGameAndModPathsIdentical();
            return false;
        }
        
        if (!Directory.Exists(baseGameDirectory))
        {
            await errorHandler.OnRootDirectoryNotFound(new RootDirectoryNotFoundErrorInfo(baseGameDirectory));
            return false;
        }

        //Check if required directories exist
        foreach (string subDirectory in RomFS.RequiredDirectories)
        {
            if (!Directory.Exists(Path.Combine(baseGameDirectory, subDirectory)))
            {
                await errorHandler.OnMissingSubDirectory(
                    new MissingSubDirectoryErrorInfo(baseGameDirectory, subDirectory)
                );
                return false;
            } 
        }
        
        //Check if root directory for mod exists
        if (modDirectory != null && !Directory.Exists(modDirectory))
        {
            await errorHandler.OnRootDirectoryNotFound(new RootDirectoryNotFoundErrorInfo(modDirectory));
            return false;
        }
        return true;
    }

    internal static async Task<string?> ResolveSystemFileLocation(string baseGameDirectory, string? modDirectory,
        RomFS.SystemFile kind,
        string[] fileLocation,
        IRomFSLoadingErrorHandler errorHandler)
    {
        string? filePath = await ResolveSystemFileLocation(baseGameDirectory, kind, fileLocation, 
            errorHandler);
            
        if (filePath == null)
            return null;

        if (modDirectory != null)
        {
            string? modFilePath = await ResolveSystemFileLocation(modDirectory, kind, fileLocation, 
                //a missing system file in the mod directory is not an error so don't report it
                errorHandler, failOnMissingSystemFile: false);
            
            if (modFilePath != null)
                filePath = modFilePath;
        }
        return filePath;
    }
    
    internal static async Task<string?> ResolveSystemFileLocation(string rootDirectory, RomFS.SystemFile kind,
        string[] fileLocation,
        IRomFSLoadingErrorHandler errorHandler, bool failOnMissingSystemFile = true)
    {
        var directoryLocation = fileLocation.AsSpan(..^1);
        string fileNamePattern = fileLocation[^1];
        
        //check if directory exists
        string directory = Path.Combine([rootDirectory, ..directoryLocation]);
        if (!Directory.Exists(directory))
        {
            if (failOnMissingSystemFile)
            {
                await errorHandler.OnMissingSystemFile(new MissingSystemFileErrorInfo(
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
            if (failOnMissingSystemFile)
            {
                await errorHandler.OnMissingSystemFile(new MissingSystemFileErrorInfo(
                    directory, fileNamePattern, 
                    kind, DirectoryExists: true));
            }
            return await Task.FromResult<string?>(null);
        }
        
        return filePath;
    }
}