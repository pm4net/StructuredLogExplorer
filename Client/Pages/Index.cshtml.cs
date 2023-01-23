using Infrastructure.Helpers;
using Infrastructure.Interfaces;
using Infrastructure.Models;

namespace StructuredLogExplorer.Pages;

public class IndexModel : PageModel
{
    private readonly IProjectService _projectService;

    public IndexModel(IProjectService projectService)
    {
        _projectService = projectService;
    }

    public IEnumerable<ProjectInfo> Projects { get; set; } = new List<ProjectInfo>();

    public void OnGet()
    {
        Projects = GetProjectInfo();
    }

    private IEnumerable<ProjectInfo> GetProjectInfo()
    {
        var projects = _projectService.GetAvailableProjects();
        var projectInfos = projects.Select(x =>
        {
            var db = _projectService.GetProjectDatabase(x, readOnly: true);
            var info = ProjectInfoHelper.GetProjectInformation(x, db);
            db.Dispose();
            return info;
        });
        return projectInfos;
    }
}
