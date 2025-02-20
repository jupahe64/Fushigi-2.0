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
                is not (true, var stageBcett, {} dataKeeper)) return (false, null);
        
        var muMap = new MuMap(muMapPath, 
            stageBcett.RefStages.HasValue ? [..dataKeeper.GetData(stageBcett.RefStages.Value)] : null
        );
        
        return (true, muMap);
    }
    
    //Todo Load stage objects with a Course loading context so all linked objects actually reference each other
    
    public IReadOnlyList<string> RefStages => _refStages ?? (IReadOnlyList<string>)Array.Empty<string>();
    
    private readonly string _muMapPath;
    private readonly List<string>? _refStages;

    private MuMap(string muMapPath, List<string>? refStages)
    {
        _muMapPath = muMapPath;
        _refStages = refStages;
    }
}