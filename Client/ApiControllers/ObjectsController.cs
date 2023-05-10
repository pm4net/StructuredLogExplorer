using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc;
using OCEL.CSharp;
using StructuredLogExplorer.Models;

namespace StructuredLogExplorer.ApiControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ObjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ObjectsController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        public IEnumerable<ObjectInfo> GetObjectTypeInfos(string projectName)
        {
            var db = _projectService.GetProjectDatabase(projectName);
            var log = OcelLiteDB.Deserialize(db);
            return log.ObjectTypes.Select(t => new ObjectInfo
            {
                Name = t,
                Occurrences = log.Objects.Count(o => o.Value.Type == t)
            });
        }
    }
}
