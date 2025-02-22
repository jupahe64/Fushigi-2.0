using Fushigi.Data;
using Fushigi.Data.Files;
using Fushigi.Data.RomFSExtensions;

namespace Fushigi.Logic.Stages;

public class Course : Stage
{
    public new static async Task<(bool success, Course? course)> Load(RomFS romFS, GymlRef stageParamGymlRef,
        IStageLoadingErrorHandler errorHandler)
    {
        if (await Stage.Load(romFS, stageParamGymlRef, errorHandler)
            is not (true, var baseInfo)) return (false, default);
        
        var course = new Course(baseInfo);

        foreach (var refStageGymlRef in course.MuMap.RefStages)
        {
            if (await Area.Load(romFS, refStageGymlRef, errorHandler)
                is not (true, {} area)) return (false, null);
            
            course._areas.Add(area);
        }
        
        return (true, course);
    }
    
    public IReadOnlyList<Area> Areas => _areas;
    private readonly List<Area> _areas = [];
    
    private Course(in StageBaseInfo baseInfo)
        : base(in baseInfo) { }
}