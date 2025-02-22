using Fushigi.Data;
using Fushigi.Data.Files;
using Fushigi.Data.Files.GymlTypes;
using Fushigi.Data.RomFSExtensions;

namespace Fushigi.Logic.Stages;

using STAGE_PARAM_REF = GymlRef<StageParam>;

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
    
    public IReadOnlyList<STAGE_PARAM_REF> RefStages => 
        _refStages ?? (IReadOnlyList<STAGE_PARAM_REF>)Array.Empty<STAGE_PARAM_REF>();
    
    private readonly MuMapRef _muMapRef;
    private readonly List<STAGE_PARAM_REF>? _refStages;

    private MuMap(MuMapRef muMapPath, List<STAGE_PARAM_REF>? refStages)
    {
        _muMapRef = muMapPath;
        _refStages = refStages;
    }
}