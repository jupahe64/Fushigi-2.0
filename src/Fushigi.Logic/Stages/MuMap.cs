using Fushigi.Data;
using Fushigi.Data.Files;
using Fushigi.Data.RomFSExtensions;

namespace Fushigi.Logic.Stages;

public sealed class MuMap
{
    public static async Task<(bool success, MuMap? muMap)> Load(
        RomFS romFS, MuMapRef muMapRef,
        IStageBcettFileLoadingErrorHandler errorHandler)
    {
        string[] bcettPath = FileRefConversion.GetRomFSFilePath(muMapRef);
        
        if (await romFS.LoadStageBcett(bcettPath, errorHandler)
                is not (true, var stageBcett, {} dataKeeper)) return (false, null);
        
        var muMap = new MuMap(muMapRef, 
            stageBcett.RefStages.HasValue ? [..dataKeeper.GetData(stageBcett.RefStages.Value)] : null
        );
        
        return (true, muMap);
    }
    
    //Todo Load stage objects with a Course loading context so all linked objects actually reference each other
    
    public IReadOnlyList<GymlRef> RefStages => _refStages ?? (IReadOnlyList<GymlRef>)Array.Empty<GymlRef>();
    
    private readonly MuMapRef _muMapRef;
    private readonly List<GymlRef>? _refStages;

    private MuMap(MuMapRef muMapPath, List<GymlRef>? refStages)
    {
        _muMapRef = muMapPath;
        _refStages = refStages;
    }
}