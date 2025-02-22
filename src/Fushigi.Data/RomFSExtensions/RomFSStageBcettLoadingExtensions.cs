using Fushigi.Data.BymlSerialization;
using Fushigi.Data.Files;
using Fushigi.Data.StageObjects;

namespace Fushigi.Data.RomFSExtensions;

public interface IStageBcettFileLoadingErrorHandler : 
    IFileResolutionAndLoadingErrorHandler, 
    IBymlDeserializeErrorHandler;

public static class RomFSStageBcettLoadingExtensions
{
    public static async Task<(bool success, StageBcett.Content content, IDeserializedStageDataKeeper? dataKeeper)> 
        LoadStageBcett(this RomFS romFS, string[] filePath,
        IStageBcettFileLoadingErrorHandler errorHandler,
        PackInfo? pack = null)
    {
        if (await romFS.LoadFile(filePath, FormatDescriptors.BymlCompressed, errorHandler)
            is not (true, { } byml, var fileLocation)) return (false, default, null);
        
        if (await StageBcett.DeserializeFrom(byml, errorHandler, fileLocation) 
            is not (true, (var content, { } dataKeeper))) return (false, default, null);
        
        return (true, content, dataKeeper);
    }
}