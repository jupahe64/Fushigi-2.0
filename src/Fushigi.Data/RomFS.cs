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
    internal static readonly string[] RequiredDirectories = ["BancMapUnit", "Gyml", "Pack", "System"];
    
    //system file locations
    private static readonly string[] s_bootupPackFileLocation = 
        "Pack/Bootup.Nin_NX_NVN.pack.zs".Split('/');

    private static readonly string[] s_addressTableFileLocation = 
        "System/AddressTable/Product.*.Nin_NX_NVN.atbl.byml.zs".Split('/');

    private static readonly string[] s_resourceSizeTableFileLocation = 
        "System/Resource/ResourceSizeTable.Product.*.rsizetable.zs".Split('/');

    public static (bool baseGameValid, bool modValid, bool isSameDirectory) ValidateRomFS(string baseGameFolderPathRomFS,
        string? modFolderPathRomFS)
    {
        bool baseGameValid = true, modValid = true, isSameDirectory = false;

        //Check if directory for basegame exists and is valid
        if (!Directory.Exists(baseGameFolderPathRomFS))
            baseGameValid = false;
        else
        {
            if (RequiredDirectories.Any(x => !Directory.Exists(Path.Combine(baseGameFolderPathRomFS, x))))
                baseGameValid = false;
        }
        
        //Check if directory for mod exists
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
        string baseGameDirectory = Path.GetFullPath(baseGameFolderPathRomFS);
        string? modDirectory = modFolderPathRomFS != null ? Path.GetFullPath(modFolderPathRomFS) : null;
        
        if (!await RomFSLoading.EnsureValid(
                baseGameDirectory, modDirectory,
                onBaseGameAndModPathsIdentical,
                onRootDirectoryNotFound,
                onMissingSubDirectory))
        {
            return (false, null);
        }

        //Load Bootup Pack
        {
            string? filePath = await RomFSLoading.ResolveSystemFileLocation(
                baseGameDirectory, modDirectory, 
                SystemFile.BootUpPack, s_bootupPackFileLocation, 
                onMissingSystemFile);
            
            if (filePath == null)
                return (false, null);

            //TODO actually load the Bootup Pack
        }
        
        //Load Resource Size Table
        {
            string? filePath = await RomFSLoading.ResolveSystemFileLocation(
                baseGameDirectory, modDirectory,
                SystemFile.ResourceSizeTable, s_resourceSizeTableFileLocation, 
                onMissingSystemFile);
            
            if (filePath == null)
                return (false, null);
            
            //TODO actually load the Resource Size Table
        }

        //Load AddressTable
        {
            string? filePath = await RomFSLoading.ResolveSystemFileLocation(
                //we assume AddressTable to never be modified and therefore ignore it if it's in the mod romFS 
                baseGameDirectory,
                SystemFile.AddressTable, s_addressTableFileLocation, 
                onMissingSystemFile);
            
            if (filePath == null)
                return (false, null);
            
            //TODO actually load the AddressTable
        }
        
        return (true, new RomFS(baseGameDirectory, modDirectory));
    }
    
    private readonly string _baseGameDirectory;
    private readonly string? _modDirectory;
    
    private RomFS(string baseGameDirectory, string? modDirectory)
    {
        _baseGameDirectory = baseGameDirectory;
        _modDirectory = modDirectory;
    }
}