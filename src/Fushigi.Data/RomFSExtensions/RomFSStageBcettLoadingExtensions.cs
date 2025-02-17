using Fushigi.Data.BymlSerialization;
using Fushigi.Data.Files;

namespace Fushigi.Data.RomFSExtensions;

public interface IStageBcettFileLoadingErrorHandler : 
    IFileResolutionAndLoadingErrorHandler, 
    IBymlDeserializeErrorHandler,
    IFileRefPathErrorHandler;

public static class RomFSStageBcettLoadingExtensions
{
    public static async Task<(bool success, StageBcett?)> LoadStageBcett(this RomFS romFS, string[] filePath,
        IStageBcettFileLoadingErrorHandler errorHandler,
        PackInfo? pack = null)
    {
        if (await romFS.LoadFile(filePath, FormatDescriptors.BymlCompressed, errorHandler)
            is not (true, { } byml)) return (false, null);
        
        if (await StageBcett.DeserializeFrom(byml, errorHandler) 
            is not (true, { } stageBcett)) return (false, null);
        
        return (true, stageBcett);
    }
}