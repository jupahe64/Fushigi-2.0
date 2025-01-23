namespace Fushigi.Data;

public record RootDirectoryNotFoundErrorInfo(string Directory);
public record MissingSubDirectoryErrorInfo(string RootDirectory, string SubDirectory);
public record MissingSystemFileErrorInfo(string Directory, string FileNamePattern, RomFS.SystemFile Kind, 
    bool DirectoryExists);

public record FileLocationResolutionErrorInfo(string[] SearchPaths, string[] SubFileLocation);
public record FileDecompressionErrorInfo(string FileName, Exception InternalException);

public class RomFS
{
    public enum SystemFile
    {
        BootUpPack,
        ResourceSizeTable,
        AddressTable
    }
    
    // just a few directories to check if a given basegame romfs folder is valid
    private static readonly string[] s_requiredDirectories = ["BancMapUnit", "Gyml", "Pack", "System"];
    
    //system file locations
    private static readonly string[] s_bootupPackFileLocation = ["Pack", "Bootup.Nin_NX_NVN.pack.zs"];
    private static readonly string[] s_addressTableFileLocation = 
        ["System", "AddressTable", "Product.*.Nin_NX_NVN.atbl.byml.zs"];
    private static readonly string[] s_resourceSizeTableFileLocation = 
        ["System", "Resource", "ResourceSizeTable.Product.*.rsizetable.zs"];

    public static (bool baseGameValid, bool modValid, bool isSameDirectory) ValidateRomFS(string baseGameFolderPathRomFS,
        string? modFolderPathRomFS)
    {
        bool baseGameValid = true, modValid = true, isSameDirectory = false;

        if (!Directory.Exists(baseGameFolderPathRomFS))
            baseGameValid = false;
        else
        {
            if (s_requiredDirectories.Any(x=> !Directory.Exists(Path.Combine(baseGameFolderPathRomFS, x))))
                baseGameValid = false;
        }
        
        // ReSharper disable once InvertIf
        if (modFolderPathRomFS != null)
        {
            if (!Directory.Exists(modFolderPathRomFS))
                modValid = false;
        
            if (Path.GetFullPath(baseGameFolderPathRomFS) == Path.GetFullPath(modFolderPathRomFS))
                isSameDirectory = true;
        }
        
        return (baseGameValid, modValid, isSameDirectory);
    }
    
    public static async Task<(bool success, RomFS? loadedRomFS)> Load(
        string baseGameFolderPathRomFS, string? modFolderPathRomFS,
        Func<Task> onBaseGameAndModPathsIdentical,
        Func<RootDirectoryNotFoundErrorInfo, Task> onRootDirectoryNotFound,
        Func<MissingSubDirectoryErrorInfo, Task> onMissingSubDirectory,
        Func<MissingSystemFileErrorInfo, Task> onMissingSystemFile)
    {
        if (modFolderPathRomFS != null &&
            Path.GetFullPath(baseGameFolderPathRomFS) == Path.GetFullPath(modFolderPathRomFS))
        {
            await onBaseGameAndModPathsIdentical();
            return (false, null);
        }
        
        if (!Directory.Exists(baseGameFolderPathRomFS))
        {
            await onRootDirectoryNotFound(new RootDirectoryNotFoundErrorInfo(baseGameFolderPathRomFS));
            return (false, null);
        }

        // ReSharper disable InvertIf
        foreach (string subDirectory in s_requiredDirectories)
        {
            if (!Directory.Exists(Path.Combine(baseGameFolderPathRomFS, subDirectory)))
            {
                await onMissingSubDirectory(new MissingSubDirectoryErrorInfo(baseGameFolderPathRomFS, subDirectory));
                return (false, null);
            } 
        }
        
        if (modFolderPathRomFS != null && !Directory.Exists(modFolderPathRomFS))
        {
            await onRootDirectoryNotFound(new RootDirectoryNotFoundErrorInfo(modFolderPathRomFS));
            return (false, null);
        }
        // ReSharper restore InvertIf

        //Load Bootup Pack
        {
            string? filePath = await ResolveSystemFileLocation(SystemFile.BootUpPack, 
                baseGameFolderPathRomFS, s_bootupPackFileLocation, onMissingSystemFile);
            
            if (filePath == null)
                return (false, null);

            if (modFolderPathRomFS != null)
            {
                string? modFilePath = await ResolveSystemFileLocation(SystemFile.BootUpPack, 
                    modFolderPathRomFS, s_bootupPackFileLocation, null);
            
                if (modFilePath != null)
                    filePath = modFilePath;
            }
            
            //TODO actually load the Bootup Pack
        }
        
        //Load Resource Size Table
        {
            string? filePath = await ResolveSystemFileLocation(SystemFile.ResourceSizeTable, 
                baseGameFolderPathRomFS, s_resourceSizeTableFileLocation, onMissingSystemFile);
            
            if (filePath == null)
                return (false, null);

            if (modFolderPathRomFS != null)
            {
                string? modFilePath = await ResolveSystemFileLocation(SystemFile.ResourceSizeTable, 
                    modFolderPathRomFS, s_resourceSizeTableFileLocation, null);
            
                if (modFilePath != null)
                    filePath = modFilePath;
            }
            
            //TODO actually load the Resource Size Table
        }

        //Load AddressTable
        {
            var filePath = await ResolveSystemFileLocation(SystemFile.AddressTable, 
                baseGameFolderPathRomFS, s_addressTableFileLocation, onMissingSystemFile);
            
            if (filePath == null)
                return (false, null);
            
            //TODO actually load the AddressTable
        }
        
        return (true, new RomFS(baseGameFolderPathRomFS, modFolderPathRomFS));
    }

    private static async Task<string?> ResolveSystemFileLocation(SystemFile kind, 
        string rootDirectory, string[] fileLocation, 
        Func<MissingSystemFileErrorInfo, Task>? onMissingSystemFile)
    {
        string directory = Path.Combine([rootDirectory, ..fileLocation[..^1]]);
        if (!Directory.Exists(directory) && onMissingSystemFile != null)
        {
            await onMissingSystemFile(new MissingSystemFileErrorInfo(
                directory, fileLocation[^1], 
                kind, Directory.Exists(directory)));
            return await Task.FromResult<string?>(null);
        }

        string fileNamePattern = fileLocation[^1];
        string? filePath = Directory.EnumerateFiles(directory, fileLocation[^1])
            .FirstOrDefault();

        // ReSharper disable once InvertIf
        if (filePath == null && onMissingSystemFile != null)
        {
            await onMissingSystemFile(new MissingSystemFileErrorInfo(
                directory, fileLocation[^1], 
                kind, DirectoryExists: true));
            return await Task.FromResult<string?>(null);
        }
        
        return filePath;
    }
    
    
    private readonly string _baseGameDirectory;
    private readonly string? _modDirectory;

    private RomFS(string baseGameDirectory, string? modDirectory)
    {
        _baseGameDirectory = baseGameDirectory;
        _modDirectory = modDirectory;
    }
}