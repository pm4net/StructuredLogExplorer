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
        var projectInfos = projects.Select(name =>
        {
            var db = _projectService.GetProjectDatabase(name);
            var info = ProjectInfoHelper.GetProjectInformation(db);
            return info;
        });
        return projectInfos;
    }
}
