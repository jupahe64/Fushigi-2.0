using Fushigi.Data;
using Fushigi.Data.Files;
using Fushigi.Data.Files.GymlTypes;

namespace Fushigi.Logic.Stages;

public class Course : Stage
{
    public new static async Task<(bool success, Course? course)> Load(RomFS romFS, 
        GymlRef<StageParam> stageParamGymlRef,
        IStageLoadingErrorHandler errorHandler)
    {
        var stageLoadContext = new MuMap.StageLoadContext();
        
        List<Area> areas = new List<Area>();

        async Task<bool> AddStageRef(GymlRef<StageParam> refStageGymlRef)
        {
            if (await Area.Load(romFS, refStageGymlRef, stageLoadContext, errorHandler)
                is not (true, {} area)) return (false);
            
            areas.Add(area);
            return true;
        }

        if (await Stage.Load(romFS, stageParamGymlRef, stageLoadContext,  AddStageRef, errorHandler)
            is not (true, var baseInfo)) return (false, default);

        var course = new Course(baseInfo, areas);
        return (true, course);
    }
    
    public IReadOnlyList<Area> Areas => _areas;
    private List<Area> _areas;
    
    private Course(in StageBaseInfo baseInfo, List<Area> areas)
        : base(in baseInfo) { 
            _areas = areas;
        }
}