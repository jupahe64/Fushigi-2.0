using Fushigi.Data;
using Fushigi.Data.Files;
using Fushigi.Data.Files.GymlTypes;
using Fushigi.Data.RomFSExtensions;

namespace Fushigi.Logic.Stages;

public class Course : Stage
{
    public CourseInfo CourseInfo { get; }
    
    public static async Task<(bool success, Course? course)> Load(RomFS romFS, 
        GymlRef<StageParam> coursestageParamRef, bool isSingleAreaCourse,
        IStageLoadingErrorHandler errorHandler)
    {
        var stageLoadContext = new MuMap.StageLoadContext();
        
        var areas = new List<Area>();

        StageBaseInfo baseInfo;
        if (isSingleAreaCourse)
        {
            if (await Stage.LoadNoMap(romFS, coursestageParamRef, errorHandler)
                is not (true, var _baseInfo)) return (false, default);
            
            if (await Area.Load(romFS, coursestageParamRef, stageLoadContext, errorHandler)
                is not (true, {} area)) return (false, default);
            
            areas.Add(area);
            
            baseInfo = _baseInfo;
        }
        else
        {
            async Task<bool> AddStageRef(GymlRef<StageParam> refStageParamRef)
            {
                await ValidateCategory(romFS, refStageParamRef, StageParam.StageCategory.Area, 
                    romFS.GetLoadedGymlFileLocation(coursestageParamRef), errorHandler);
                
                if (await Area.Load(romFS, refStageParamRef, stageLoadContext, errorHandler)
                    is not (true, {} area)) return (false);
            
                areas.Add(area);
                return true;
            }
            
            if (await Stage.Load(romFS, coursestageParamRef, stageLoadContext, AddStageRef, errorHandler)
                is not (true, var _baseInfo)) return (false, default);
            
            baseInfo = _baseInfo;
        }
        
        
        if (await baseInfo.GetComponent("CourseInfo", x => ref x._.CourseInfo, errorHandler) 
            is not (true, { } courseInfoRef)) return (false, default);
        
        if (await romFS.LoadGyml(courseInfoRef, errorHandler) is not (true, { } courseInfo))
            return (false, default);

        var course = new Course(baseInfo, areas, courseInfo);
        return (true, course);
    }
    
    public IReadOnlyList<Area> Areas => _areas;
    private List<Area> _areas;

    private Course(in StageBaseInfo baseInfo, List<Area> areas, CourseInfo courseInfo)
        : base(in baseInfo)
    {
        _areas = areas;
        CourseInfo = courseInfo;
    }
}