using Fushigi.Data;
using Fushigi.Data.Files;
using Fushigi.Data.RomFSExtensions;

namespace Fushigi.Logic.Stages;

public sealed class MuMap
{
    public static async Task<(bool success, MuMap? muMap)> Load(
        RomFS romFS, string muMapPath,
        IStageBcettFileLoadingErrorHandler errorHandler)
    {
        if (await FileRefPathConversion.GetRomFSFilePath(muMapPath, (".mumap", ".bcett.byml.zs"), errorHandler)
                is not (true, { } bcettPath)) return (false, null);
        
        if (await romFS.LoadStageBcett(bcettPath, errorHandler)
                is not (true, { } stageBcett)) return (false, null);
        
        var muMap = new MuMap(muMapPath, stageBcett);
        
        return (true, muMap);
    }
    
    //Todo Load stage objects with a Course loading context so all linked objects actually reference each other
    
    public IReadOnlyList<string> RefStages => _stageBcett.RefStages;
    
    private readonly string _muMapPath;
    private readonly StageBcett _stageBcett;

    private MuMap(string muMapPath, StageBcett stageBcett)
    {
        _muMapPath = muMapPath;
        _stageBcett = stageBcett;
    }
}