using FSharpx;
using Infrastructure.Helpers;
using Infrastructure.Interfaces;
using Infrastructure.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.FSharp.Core;
using pm4net.Algorithms.Discovery.Ocel;
using OCEL.CSharp;
using pm4net.Types.Trees;
using pm4net.Utilities;
using pm4net.Types;
using StructuredLogExplorer.Models.ControllerOptions;
using NodeInfo = pm4net.Types.NodeInfo;

namespace StructuredLogExplorer.ApiControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MapController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly IGraphLayoutService _graphLayoutService;
        private readonly IDictionary<string, OcelLog> _logs;

        public MapController(IProjectService projectService, IGraphLayoutService graphLayoutService)
        {
            _projectService = projectService;
            _graphLayoutService = graphLayoutService;
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
                var db = _projectService.GetProjectDatabase(projectName);
                log = OcelLiteDB.Deserialize(db);
                _logs.Add(projectName, log);
            }

            return log;
        }

        /// <summary>
        /// Discover an object-centric directly-follows-graph (OC-DFG) from a project's event log, applying the given options.
        /// </summary>
        private GraphTypes.DirectedGraph<InputTypes.Node<NodeInfo>, InputTypes.Edge<EdgeInfo>> DiscoverOriginalOcDfg(string projectName, OcDfgOptions options)
        {
            var log = GetProjectLog(projectName);
            return OcelDfg.Discover(new OcDfgFilter(options.MinimumEvents, options.MinimumOccurrence, options.MinimumSuccessions,
                    options.DateFrom != null && options.DateTo != null
                        ? FSharpOption<TimeframeFilter>.Some(new TimeframeFilter(options.DtoFrom!.Value.StartOfDay(), options.DtoTo!.Value.EndOfDay(), options.KeepCases.ToPm4Net()))
                        : FSharpOption<TimeframeFilter>.None),
                options.IncludedTypes, log);
        }

        [HttpGet]
        [Route("getLogInfo")]
        public LogInfo GetLogInfo(string projectName)
        {
            var log = GetProjectLog(projectName);
            return new LogInfo
            {
                ObjectTypes = log.ObjectTypes.Order(),
                FirstEventTimestamp = log.OrderedEvents.FirstOrDefault().Value.Timestamp.ToString("dd/MM/yyyy"),
                LastEventTimestamp = log.OrderedEvents.LastOrDefault().Value.Timestamp.ToString("dd/MM/yyyy")
            };
        }

        [HttpGet]
        [Route("getAllNodesInLog")]
        public IEnumerable<LogNode> GetAllNodesInLog(string projectName)
        {
	        var log = GetProjectLog(projectName);
	        return log.Events
		        .Select(x => new LogNode(x.Value.Activity, x.Value.Activity, new Event()))
		        .Concat(log.ObjectTypes.Select(x => new LogNode(Constants.objectTypeStartNode + x, x, new Start())))
		        .Concat(log.ObjectTypes.Select(x => new LogNode(Constants.objectTypeEndNode + x, x, new End())))
		        .DistinctBy(x => x.Id);
        }

        [HttpPost]
        [Route("saveNodeCalculations")]
        public void SaveNodeCalculations(string projectName, [FromBody] IEnumerable<NodeCalculation> calculations)
        {
            _graphLayoutService.SaveNodeCalculations(projectName, calculations);
        }

        [HttpPost]
        [Route("computeLayoutWithModel")]
		public GraphLayout ComputeLayoutWithModel(string projectName, [FromBody] (GraphTypes.DirectedGraph<InputTypes.Node<NodeInfo>, InputTypes.Edge<EdgeInfo>>, GraphLayoutOptions) modelAndOptions)
        {
	        var log = GetProjectLog(projectName);
	        return _graphLayoutService.ComputeGraphLayout(projectName, log, modelAndOptions.Item1,
		        modelAndOptions.Item2.MergeEdgesOfSameType, modelAndOptions.Item2.FixUnforeseenEdges,
		        modelAndOptions.Item2.NodeSeparation, modelAndOptions.Item2.RankSeparation,
		        modelAndOptions.Item2.EdgeSeparation, modelAndOptions.Item2.Tension);
        }

		[HttpPost]
		[Route("computeLayout")]
		public GraphLayout ComputeLayout(string projectName, [FromBody] OcDfgLayoutOptions options)
		{
			var model = DiscoverOriginalOcDfg(projectName, options.OcDfgOptions);
			return ComputeLayoutWithModel(projectName, (model, options.LayoutOptions));
		}

        [HttpPost]
        [Route("discoverOcDfg")]
        public GraphLayout DiscoverOcDfg(string projectName, [FromBody] OcDfgOptions options)
        {
            var ocDfg = DiscoverOriginalOcDfg(projectName, options);
            var nodeCalculations = _graphLayoutService.GetNodeCalculations(projectName)?.ToList();
            if (nodeCalculations is null)
            {
                throw new ArgumentNullException(nameof(nodeCalculations), "Node calculations have not been made yet.");
            }

            return ocDfg.FromOcDfg(nodeCalculations);
        }

        [HttpPost]
        [Route("discoverOcDfgAndDot")]
        public string DiscoverOcDfgAndGenerateDot(string projectName, bool groupByNamespace, [FromBody] OcDfgOptions options)
        {
	        var ocDfg = DiscoverOriginalOcDfg(projectName, options);
            var dot = pm4net.Visualization.Ocel.Graphviz.OcDfg2Dot(ocDfg, groupByNamespace);
            return dot;
        }

        [HttpGet]
        [Route("getTracesForObjectType")]
		public IEnumerable<(OcelObject, IEnumerable<(string, Infrastructure.Models.OcelEvent)>)> GetTracesForObjectType(string projectName, string objectType)
		{
	        var log = GetProjectLog(projectName);
	        var flattened = OcelHelpers.Flatten(log.ToFSharpOcelLog(), objectType);
	        var traces = OcelHelpers.OrderedTracesOfFlattenedLog(flattened);
			return traces.Select(t => (t.Item1.FromFSharpOcelObject(), t.Item2.Select(e => (e.Item1, e.Item2.FromRegularOcelEvent(flattened)))));
		}

        [HttpGet]
        [Route("namespaceTree")]
        [OutputCache] // TODO: Invalidate in FileController when new log files are imported
		[Obsolete("Not currently used")]
		public ListTree<string> GetNamespaceTree(string projectName)
        {
            // TODO: Port ListTree type to C# type that NSwag can convert
            var log = GetProjectLog(projectName);
            var namespaces = log.Events.Select(e => OcelHelpers.GetNamespace(e.Value.ToFSharpOcelEvent())).Distinct();
            return OcelHelpers.NamespaceTree(new[] {'.'}, namespaces.Select(n => n.Value)); // TODO: Check if null first
        }
    }
}
