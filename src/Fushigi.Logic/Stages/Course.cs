using Fushigi.Data;
using Fushigi.Data.RomFSExtensions;

namespace Fushigi.Logic.Stages;

public class Course : Stage
{
    public new static async Task<(bool success, Course? course)> Load(RomFS romFS, string stageParamGymlPath,
        IGymlFileLoadingErrorHandler errorHandler)
    {
        if (await Stage.Load(romFS, stageParamGymlPath, errorHandler)
            is not (true, var baseInfo)) return (false, default);
        
        var course = new Course(baseInfo);

        foreach (string refStageGymlPath in course.MuMap.RefStages)
        {
            if (await Area.Load(romFS, refStageGymlPath, errorHandler)
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