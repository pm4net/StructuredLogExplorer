using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using pm4net.Algorithms.Discovery.Ocel;
using OCEL.CSharp;
using pm4net.Types.Trees;
using pm4net.Utilities;
using pm4net.Algorithms.Layout;
using pm4net.Types.GraphLayout;
using StructuredLogExplorer.Models;
using Node = StructuredLogExplorer.Models.Node;

namespace StructuredLogExplorer.ApiControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MapController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly IDictionary<string, OcelLog> _logs;

        public MapController(IProjectService projectService)
        {
            _projectService = projectService;
            _logs = new Dictionary<string, OcelLog>();
        }

        // TODO: Remove entries when logs are imported (if needed, since controllers are transient, so limited in lifetime)
        private OcelLog GetProjectLog(string projectName)
        {
            OcelLog log;
            if (_logs.ContainsKey(projectName))
            {
                log = _logs[projectName];
            }
            else
            {
                var db = _projectService.GetProjectDatabase(projectName, readOnly: true);
                log = OcelLiteDB.Deserialize(db);
                _logs.Add(projectName, log);
            }

            return log;
        }

        [HttpGet]
        [Route("getLogInfo")]
        [OutputCache] // TODO: Invalidate in FileController when new log files are imported
        public LogInfo GetLogInfo(string projectName)
        {
            var log = GetProjectLog(projectName);
            return new LogInfo { ObjectTypes = log.ObjectTypes };
        }

        [HttpGet]
        [Route("discoverOcDfg")]
        [OutputCache] // TODO: Invalidate in FileController when new log files are imported
        public DirectedGraph<Node, Edge> DiscoverObjectCentricDirectlyFollowsGraph(
            string projectName,
            int minEvents,
            int minOccurrences, 
            int minSuccessions,
            [FromQuery] IEnumerable<string> includedTypes)
        {
            var log = GetProjectLog(projectName);
            return OcelDfg.Discover(minEvents, minOccurrences, minSuccessions, includedTypes, log).FromFSharpGraph();
        }

        [HttpGet]
        [Route("discoverOcDfgAndApplyStableGraphLayout")]
        [OutputCache] // TODO: Invalidate in FileController when new log files are imported
        public DirectedGraph<Node, Edge> DiscoverOcDfgAndApplyStableGraphLayout(
            string projectName,
            bool groupByNamespace,
            int minEvents,
            int minOccurrences,
            int minSuccessions,
            [FromQuery] IEnumerable<string> includedTypes)
        {
            var log = GetProjectLog(projectName);
            var ocDfg = OcelDfg.Discover(minEvents, minOccurrences, minSuccessions, includedTypes, log);
            var globalOrder = StableGraphLayout.ComputeGlobalOrder(log, ocDfg);
            return ocDfg.FromFSharpGraph().EnrichWithGlobalOrder(globalOrder);
        }

        [HttpGet]
        [Route("discoverOcDfgAndDot")]
        [OutputCache] // TODO: Invalidate in FileController when new log files are imported
        public string DiscoverOcDfgAndGenerateDot(
            string projectName,
            bool groupByNamespace,
            int minEvents,
            int minOccurrences,
            int minSuccessions,
            [FromQuery] IEnumerable<string> includedTypes)
        {
            var log = GetProjectLog(projectName);
            var ocDfg = OcelDfg.Discover(minEvents, minOccurrences, minSuccessions, includedTypes, log);
            var dot = pm4net.Visualization.Ocel.Graphviz.OcDfg2Dot(ocDfg, groupByNamespace);
            return dot;
        }

        [HttpGet]
        [Route("namespaceTree")]
        [OutputCache] // TODO: Invalidate in FileController when new log files are imported
        public ListTree<string> GetNamespaceTree(string projectName)
        {
            // TODO: Port ListTree type to C# type that NSwag can convert
            var log = GetProjectLog(projectName);
            var namespaces = log.Events.Select(e => OcelHelpers.GetNamespace(e.Value.ToFSharpOcelEvent())).Distinct();
            return OcelHelpers.NamespaceTree(new[] {'.'}, namespaces);
        }
    }
}
