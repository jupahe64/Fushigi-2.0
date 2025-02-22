using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using BymlLibrary;
using RstbLibrary;
using SarcLibrary;
using ZstdSharp;
// ReSharper disable ConvertIfStatementToReturnStatement

namespace Fushigi.Data;

// info: "FilePath" and alike here always refers to an array of path nodes
//       that describe the path to a file from a given root node
//       a file path that describes an ABSOLUTE location of a file is referred to as "FilePathFS"

public record RootDirectoryNotFoundErrorInfo(string Directory);
public record MissingSubDirectoryErrorInfo(string RootDirectory, string SubDirectory);
public record MissingSystemFileErrorInfo(string Directory, string FileNamePattern, RomFS.SystemFile Kind, 
    bool DirectoryExists);
public record FileDecompressionErrorInfo(string FilePathFS, Exception InternalException);
public record FileFormatReaderErrorInfo(RomFS.RetrievedFileLocation FileLocation, Exception InternalException);

public record FilePathResolutionErrorInfo((string path, bool isArchive)[] SearchPathsFS, string[] FilePath);

public interface IRomFSLoadingErrorHandler : IFileLoadingErrorHandler
{
    Task OnBaseGameAndModPathsIdentical();
    Task OnRootDirectoryNotFound(RootDirectoryNotFoundErrorInfo info);
    Task OnMissingSubDirectory(MissingSubDirectoryErrorInfo info);
    Task OnMissingSystemFile(MissingSystemFileErrorInfo info); 
}

public interface IFileResolutionAndLoadingErrorHandler : IFileLoadingErrorHandler
{
    Task OnFileNotFound(FilePathResolutionErrorInfo info);
}

public interface IFileLoadingErrorHandler
{
    Task OnFileDecompressionFailed(FileDecompressionErrorInfo info);
    Task OnFileReadFailed(FileFormatReaderErrorInfo info);
}

public record struct FileFormatDescriptor<TFormat>(bool IsCompressed, 
    Func<ArraySegment<byte>, TFormat> Reader, 
    Func<ArraySegment<byte>, TFormat>? Writer = null);

public class PackInfo
{
    internal Sarc Arc { get; }
    internal string FilePathFS { get; }

    internal PackInfo(Sarc arc, string filePathFS)
    {
        FilePathFS = filePathFS;
        Arc = arc;
    }
}

public class RomFS
{
    public readonly record struct RetrievedFileLocation
    {
        public bool IsInPack(out (string packFilePath, string[] inPackSubPath) fileLocation)
        {
            if (_fileInPackPath != null)
            {
                fileLocation = (_filePathFS, _fileInPackPath);
                return true;
            }

            fileLocation = ("", []);
            return false;
        }
        
        public bool IsInFileSystem([NotNullWhen(true)] out string? filePath)
        {
            if (_fileInPackPath != null)
            {
                filePath = _filePathFS;
                return true;
            }

            filePath = null;
            return false;
        }

        public record struct InPackInfo(string PackFilePath, string[] InPackSubPath);
        public record struct InFileSystemInfo(string FilePath);
        
        public InPackInfo? InPack => IsInPack(out var fileLocation) ? 
            new InPackInfo(fileLocation.packFilePath, fileLocation.inPackSubPath) : null;
        public InFileSystemInfo? InFileSystem => IsInFileSystem(out string? filePath) ? 
            new InFileSystemInfo(filePath) : null;

        internal static RetrievedFileLocation OfInPack(string packFilePath, string[] inPackSubPath)
        {
            return new RetrievedFileLocation(packFilePath, inPackSubPath);
        }
        
        internal static RetrievedFileLocation OfInFileSystem(string filePath)
        {
            return new RetrievedFileLocation(filePath, null);
        }

        private RetrievedFileLocation(string filePathFS, string[]? fileInPackPath)
        {
            _filePathFS = filePathFS;
            _fileInPackPath = fileInPackPath;
        }
        
        private readonly string _filePathFS;
        private readonly string[]? _fileInPackPath;
    }
    
    public enum SystemFile
    {
        BootUpPack,
        ResourceSizeTable,
        AddressTable
    }
    
    // just a few directories to check if a given basegame romfs folder is valid
    internal static readonly string[] RequiredDirectories = ["BancMapUnit", "Gyml", "Pack", "System"];
    
    //system file locations
    private static readonly string[] s_bootupPackFilePath = 
        "Pack/Bootup.Nin_NX_NVN.pack.zs".Split('/');

    private static readonly string[] s_addressTableFilePath = 
        "System/AddressTable/Product.*.Nin_NX_NVN.atbl.byml.zs".Split('/');

    private static readonly string[] s_resourceSizeTableFilePath = 
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
        IRomFSLoadingErrorHandler errorHandler)
    {
        string baseGameDirectory = Path.GetFullPath(baseGameFolderPathRomFS);
        string? modDirectory = modFolderPathRomFS != null ? Path.GetFullPath(modFolderPathRomFS) : null;
        
        if (!await RomFSLoading.EnsureValid(
                baseGameDirectory, modDirectory,
                errorHandler))
        {
            return (false, null);
        }

        //Load Bootup Pack
        string bootupPackFilePathFS;
        Sarc bootupPack;
        {
            const SystemFile kind = SystemFile.BootUpPack;
            string[] filePath = s_bootupPackFilePath;
            
            string? filePathFS = await RomFSLoading.ResolveSystemFileLocation(
                baseGameDirectory, modDirectory, 
                kind, filePath, 
                errorHandler);
            
            if (filePathFS == null)
                return (false, null);
            
            if (await RomFSFileLoading.LoadFileFromFS(filePathFS, s_packFormatDescriptor, errorHandler)
                is not ({} _bootupPack, exists: true, success: true))
                return (false, null);
            
            bootupPack = _bootupPack;
            bootupPackFilePathFS = filePathFS;
        }
        
        //Load Resource Size Table
        Rstb resourceSizeTable;
        {
            const SystemFile kind = SystemFile.ResourceSizeTable;
            string[] filePath = s_resourceSizeTableFilePath;
            
            string? filePathFS = await RomFSLoading.ResolveSystemFileLocation(
                baseGameDirectory, modDirectory,
                kind, filePath, 
                errorHandler);
            
            if (filePathFS == null)
                return (false, null);
            
            if (await RomFSFileLoading.LoadFileFromFS(filePathFS, s_resourceSizeTableFormat, errorHandler)
                is not ({} _resourceSizeTable, exists: true, success: true))
                return (false, null);
            
            resourceSizeTable = _resourceSizeTable;
        }

        //Load AddressTable
        Dictionary<string, string> addressTable;
        {
            const SystemFile kind = SystemFile.AddressTable;
            string[] filePath = s_addressTableFilePath;
            
            string? filePathFS = await RomFSLoading.ResolveSystemFileLocation(
                //we assume AddressTable to never be modified and therefore ignore it if it's in the mod romFS 
                baseGameDirectory,
                kind, filePath, 
                errorHandler);
            
            if (filePathFS == null)
                return (false, null);

            if (await RomFSFileLoading.LoadFileFromFS(filePathFS, s_addressTableFormat, errorHandler)
                is not ({} byml, exists: true, success: true))
                return (false, null);
            
            addressTable = [];
            foreach ((string key, var value) in byml.GetMap())
                addressTable[key] = value.GetString();
        }
        
        return (true, new RomFS(baseGameFolderPathRomFS, modFolderPathRomFS, 
            new PackInfo(bootupPack, bootupPackFilePathFS), 
            resourceSizeTable, addressTable));
    }

    public async Task<(bool success, PackInfo? pack)> LoadPack(string[] filePath,
        IFileResolutionAndLoadingErrorHandler errorHandler)
    {
        string? checkedModFSFile = null, checkedRomFSFile;
        
        // try load from modFS
        if (_modDirectory != null)
        {
            string filePathFS = checkedModFSFile = Path.Combine([_modDirectory, ..filePath]);
            if (await RomFSFileLoading.LoadFileFromFS(filePathFS, s_packFormatDescriptor, errorHandler) 
                is (var file, exists: true, var success))
            {
                Debug.Assert(!success || file != null); //success => file not null
                return success ? (true, new PackInfo(file!, filePathFS)) : (false, null);
            }
        }
        
        // try load from romFS
        {
            string filePathFS = checkedRomFSFile = Path.Combine([_baseGameDirectory, ..filePath]);
            if (await RomFSFileLoading.LoadFileFromFS(filePathFS, s_packFormatDescriptor, errorHandler) 
                is (var file, exists: true, var success))
            {
                Debug.Assert(!success || file != null); //success => file not null
                return success ? (true, new PackInfo(file!, filePathFS)) : (false, null);
            }
        }
        
        // file resolution failed, generate the error info and report
        await errorHandler.OnFileNotFound(GenerateFilePathResolutionErrorInfo(
            null, null, checkedModFSFile, checkedRomFSFile, filePath));
        return (false, null);
    }

    public async Task<(bool success, TFormat? content, RetrievedFileLocation location)> LoadFile<TFormat>(
        string[] filePath, 
        FileFormatDescriptor<TFormat> format,
        IFileResolutionAndLoadingErrorHandler errorHandler,
        PackInfo? pack = null)
        where TFormat : class
    {
        // general approach for loading a file is:
        // 1. lookup filePath in addressTable and overwrite when necessary
        // 2. search in given pack (if given), then bootup, then modFS (if set), then romFS
        // 2.a if a file was found, try to open it, report and cancel on error (decompressing, reading format)
        // 2.b if file was not found (in any search location), report
        
        //this can be optimized but it's fine for now
        if (_addressTable.TryGetValue(string.Join('/', filePath), out string? addressTableEntry))
            filePath = addressTableEntry.Split('/');
        
        // most of the code in this function is boilerplate (that's why it's in collapsable regions)
        // the actual loading is done in the RomFSFileLoading class

        string? checkedGivenPackFile = null, checkedBootupPackFile = null;

        #region Check Packs
        // check given pack
        if (!format.IsCompressed && pack != null)
        {
            checkedGivenPackFile = pack.FilePathFS;
            if (await RomFSFileLoading.LoadFileFromPack(filePath, pack, format.Reader, errorHandler) 
                is (var file, exists: true, var success))
            {
                Debug.Assert(!success || file != null); //success => file not null
                return success ? (true, file, RetrievedFileLocation.OfInPack(pack.FilePathFS, filePath)) 
                    : (false, null, default);
            }
        }
        // check bootup pack
        if (!format.IsCompressed) //a bit ugly but nesting is uglier
        {
            checkedBootupPackFile = _bootupPack.FilePathFS;
            if (await RomFSFileLoading.LoadFileFromPack(filePath, _bootupPack, format.Reader, errorHandler) 
                is (var file, exists: true, var success))
            {
                Debug.Assert(!success || file != null); //success => file not null
                return success ? (true, file, RetrievedFileLocation.OfInPack(_bootupPack.FilePathFS, filePath)) 
                    : (false, null, default);
            }
        }
        #endregion
        
        string checkedModFSFile = null, checkedRomFSFile;
        #region Check FileSystem
        
        // check modFS
        if (_modDirectory != null)
        {
            string filePathFS = checkedModFSFile = Path.Combine([_modDirectory, ..filePath]);
            if (await RomFSFileLoading.LoadFileFromFS(filePathFS, format, errorHandler) 
                is (var file, exists: true, var success))
            {
                Debug.Assert(!success || file != null); //success => file not null
                return success ? (true, file, RetrievedFileLocation.OfInFileSystem(filePathFS)) : (false, null, default);
            }
        }
        //check romFS
        {
            string filePathFS = checkedRomFSFile = Path.Combine([_baseGameDirectory, ..filePath]);
            if (await RomFSFileLoading.LoadFileFromFS(filePathFS, format, errorHandler)
                is (var file, exists: true, var success))
            {
                Debug.Assert(!success || file != null); //success => file not null
                return success ? (true, file, RetrievedFileLocation.OfInFileSystem(filePathFS)) : (false, null, default);
            }
        }
        #endregion
        
        // file resolution failed, generate the error info and report
        await errorHandler.OnFileNotFound(GenerateFilePathResolutionErrorInfo(
            checkedGivenPackFile, checkedBootupPackFile, checkedModFSFile, checkedRomFSFile, filePath));

        return (false, null, default);
    }
    
    private static FilePathResolutionErrorInfo GenerateFilePathResolutionErrorInfo(
        string? checkedPackFile1, string? checkedPackFile2, string? checkedModFSFile, string? checkedRomFSFile, 
        string[] filePath)
    {
        List<(string path, bool isArchive)> searchPaths = [];
        
        if (checkedPackFile1 != null)
            searchPaths.Add((checkedPackFile1, true));
        if (checkedPackFile2 != null)
            searchPaths.Add((checkedPackFile2, true));
        
        //these should always give us the path without filename
        if (checkedModFSFile != null)
            searchPaths.Add((Path.GetDirectoryName(checkedModFSFile)!, false));
        
        searchPaths.Add((Path.GetDirectoryName(checkedRomFSFile)!, false));
        return new FilePathResolutionErrorInfo(searchPaths.ToArray(), filePath);
    }
    
    private readonly string _baseGameDirectory;
    private readonly string? _modDirectory;
    
    private readonly Dictionary<string, string> _addressTable;
    private readonly PackInfo _bootupPack;
    private readonly Rstb _resourceSizeTable;
    
    private static readonly FileFormatDescriptor<Sarc> s_packFormatDescriptor
        = new(IsCompressed: true, Reader: Sarc.FromBinary);
    
    private static readonly FileFormatDescriptor<Byml> s_addressTableFormat
        = new(IsCompressed: true, b => Byml.FromBinary(b));
    
    private static readonly FileFormatDescriptor<Rstb> s_resourceSizeTableFormat
        = new(IsCompressed: true, b => Rstb.FromBinary(b));
    
    private static readonly Compressor s_zsCompressor = new Compressor(17);

    private RomFS(string baseGameDirectory, string? modDirectory, 
        PackInfo bootupPack, Rstb resourceSizeTable, Dictionary<string, string> addressTable)
    {
        _baseGameDirectory = baseGameDirectory;
        _modDirectory = modDirectory;
        _bootupPack = bootupPack;
        _resourceSizeTable = resourceSizeTable;
        _addressTable = addressTable;
    }
}