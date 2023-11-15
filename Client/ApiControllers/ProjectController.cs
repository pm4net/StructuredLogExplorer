using Infrastructure.Helpers;
using Infrastructure.Interfaces;
using Infrastructure.Models;
using Microsoft.AspNetCore.Mvc;

namespace StructuredLogExplorer.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        [HttpGet]
        [Route("projects")]
        public IEnumerable<ProjectInfo> GetProjects()
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

        [HttpGet]
        [Route("create")]
        public void Create(string name, string logDir)
        {
            _projectService.CreateProject(name, logDir);
        }

        [HttpDelete]
        [Route("delete")]
        public void Delete(string name)
        {
            _projectService.DeleteProject(name);
        }
    }
}
