using ZstdSharp;

namespace Fushigi.Data;

public static class RomFSFileLoading
{
    internal static async Task<(TFormat? file, bool exists, bool success)> LoadFileFromFS<TFormat>(
        string filePathFS, 
        FileFormatDescriptor<TFormat> format,
        IFileLoadingErrorHandler errorHandler)
        where TFormat : class
    {
        if (!File.Exists(filePathFS))
            return (null, exists: false, success: false);

        ArraySegment<byte> bytes;
        if (!format.IsCompressed)
            bytes = await File.ReadAllBytesAsync(filePathFS);
        else
        {
            if (await DecompressFile(filePathFS,
                    errorHandler
                ) is not (true, { } _bytes))
                return (null, exists: true, success: false);

            bytes = _bytes;
        }

        var result = await ReadFileFormat(bytes, format.Reader, errorHandler, 
            RomFS.RetrievedFileLocation.OfInFileSystem(filePathFS));
        return (result.file, exists: true, result.success);
    }

    internal static async Task<(TFormat? file, bool exists, bool success)> LoadFileFromPack<TFormat>(
        string[] filePath,
        PackInfo pack, Func<ArraySegment<byte>, TFormat> formatReader,
        IFileLoadingErrorHandler errorHandler)
        where TFormat : class
    {
        if (!pack.Arc.TryGetValue(string.Join('/', filePath), out var bytes))
            return (null, exists: false, success: false);
        
        // we assume files in packs to not be compressed
        // as the packs themselves are already compressed so we can just read the stored bytes
        var result = await ReadFileFormat(bytes, formatReader, errorHandler, 
            RomFS.RetrievedFileLocation.OfInPack(pack.FilePathFS, filePath));
        return (result.file, exists: true, result.success);
    }
    
    private static async Task<(bool success, byte[]? content)> DecompressFile(string filePathFS,
        IFileLoadingErrorHandler errorHandler)
    {
        byte[] compressedData = await File.ReadAllBytesAsync(filePathFS);
        byte[] uncompressedData;
        try
        {
            uncompressedData = new byte[Decompressor.GetDecompressedSize(compressedData)];
             s_zsDecompressor.Unwrap(compressedData, uncompressedData);
        }
        catch (ZstdException ex)
        {
            await errorHandler.OnFileDecompressionFailed(new FileDecompressionErrorInfo(filePathFS, ex));
            return (false, null);
        }
        return (true, uncompressedData);
    }
    
    private static async Task<(bool success, TFormat? file)> ReadFileFormat<TFormat>(
        ArraySegment<byte> bytes, Func<ArraySegment<byte>, TFormat> formatReader, 
        IFileLoadingErrorHandler errorHandler, 
        RomFS.RetrievedFileLocation location)
        where TFormat : class
    {
        TFormat? file;
        try
        {
            file = formatReader(bytes);
        }
        // we don't know what exception the reader might throw,
        // but we definitely want to catch it to present it to the user
        catch (Exception e) 
        {
            await errorHandler.OnFileReadFailed(new FileFormatReaderErrorInfo(location , e));
            return (false, null);
        }
        
        return (true, file);
    }

    private static readonly Decompressor s_zsDecompressor = new Decompressor();
}