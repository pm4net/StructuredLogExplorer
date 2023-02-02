using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc;
using pm4net.Algorithms.Discovery.Ocel;
using OCEL.CSharp;
using StructuredLogExplorer.Models;

namespace StructuredLogExplorer.ApiControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MapController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public MapController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        [HttpGet]
        [Route("discoverOcDfg")]
        public DirectedGraph<Node, Edge> DiscoverObjectCentricDirectlyFollowsGraph(
            string projectName,
            int minEvents,
            int minOccurrences, 
            int minSuccessions,
            [FromQuery] IEnumerable<string> includedTypes)
        {
            var db = _projectService.GetProjectDatabase(projectName, readOnly: true);
            var log = OcelLiteDB.Deserialize(db);
            return OcelDfg.Discover(minEvents, minOccurrences, minSuccessions, includedTypes, log).FromFSharpGraph();
        }
    }
}
