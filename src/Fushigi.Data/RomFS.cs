using BymlLibrary;
using RstbLibrary;
using SarcLibrary;
using ZstdSharp;

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
        
        Sarc bootupPack;
        Rstb resourceSizeTable;
        Dictionary<string, string> addressTable;

        //Load Bootup Pack
        {
            string? filePath = await RomFSLoading.ResolveSystemFileLocation(
                baseGameDirectory, modDirectory, 
                SystemFile.BootUpPack, s_bootupPackFileLocation, 
                onMissingSystemFile);
            
            if (filePath == null)
                return (false, null);
            
            bootupPack = Sarc.FromBinary(Decompress(await File.ReadAllBytesAsync(filePath)));
        }
        
        //Load Resource Size Table
        {
            string? filePath = await RomFSLoading.ResolveSystemFileLocation(
                baseGameDirectory, modDirectory,
                SystemFile.ResourceSizeTable, s_resourceSizeTableFileLocation, 
                onMissingSystemFile);
            
            if (filePath == null)
                return (false, null);
            
            resourceSizeTable = Rstb.FromBinary(Decompress(await File.ReadAllBytesAsync(filePath)));
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
            
            var byml = Byml.FromBinary(Decompress(await File.ReadAllBytesAsync(filePath)));

            addressTable = [];
            foreach ((string key, var value) in byml.GetMap())
                addressTable[key] = value.GetString();
        }
        
        return (true, new RomFS(baseGameFolderPathRomFS, modFolderPathRomFS, 
            bootupPack, resourceSizeTable, addressTable));
    }

    private static byte[] Decompress(byte[] compressedData)
    {
        var uncompressedData = new byte[Decompressor.GetDecompressedSize(compressedData)];
        s_zsDecompressor.Unwrap(compressedData, uncompressedData);
        return uncompressedData;
    }
    
    private readonly string _baseGameDirectory;
    private readonly string? _modDirectory;
    
    private Dictionary<string, string> _addressTable;
    private Sarc _bootupPack;
    private Rstb _resourceSizeTable;
    
    private static readonly Decompressor s_zsDecompressor = new Decompressor();
    private static readonly Compressor s_zsCompressor = new Compressor(17);

    private RomFS(string baseGameDirectory, string? modDirectory, 
        Sarc bootupPack, Rstb resourceSizeTable, Dictionary<string, string> addressTable)
    {
        _baseGameDirectory = baseGameDirectory;
        _modDirectory = modDirectory;
        _bootupPack = bootupPack;
        _resourceSizeTable = resourceSizeTable;
        _addressTable = addressTable;
    }
}