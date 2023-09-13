using FSharpx;
using Infrastructure.Helpers;
using Infrastructure.Interfaces;
using Infrastructure.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using pm4net.Algorithms.Discovery.Ocel;
using OCEL.CSharp;
using pm4net.Types.Trees;
using pm4net.Utilities;
using pm4net.Types;
using StructuredLogExplorer.Models.ControllerOptions;
using NodeInfo = pm4net.Types.NodeInfo;
using OcelEvent = Infrastructure.Models.OcelEvent;

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
        private pm4net.Types.DirectedGraph<Node<NodeInfo>, Edge<EdgeInfo>> DiscoverOriginalOcDfg(string projectName, OcDfgOptions options)
        {
            var log = GetProjectLog(projectName);

            // TODO: Only includes high-level namespaces for now
            var nsTree = ListTree<string>.NewNode("", 
                options.IncludedNamespaces.Select(ns => ListTree<string>.NewNode(ns, 
                    new List<ListTree<string>> { ListTree<string>.NewNode("*", 
                        FSharpList<ListTree<string>>.Empty) }.ToFSharpList()
                    )).ToFSharpList());

            return OcelDfg.Discover(new OcDfgFilter(options.MinimumEvents, options.MinimumOccurrence, options.MinimumSuccessions,
                    options.DateFrom != null && options.DateTo != null
                        ? FSharpOption<TimeframeFilter>.Some(new TimeframeFilter(options.DtoFrom!.Value.StartOfDay(), options.DtoTo!.Value.EndOfDay(), options.KeepCases.ToPm4Net()))
                        : FSharpOption<TimeframeFilter>.None, options.IncludedLogLevels.Select(l => l.ToFSharpLogLevel()).ToFSharpList(), options.IncludedNamespaces.Any() ? nsTree : FSharpOption<ListTree<string>>.None),
                options.IncludedTypes, log);
        }

        [HttpGet]
        [Route("getLogInfo")]
        public LogInfo GetLogInfo(string projectName)
        {
            var log = GetProjectLog(projectName);
            var namespaces = log.Events.Select(e => OcelHelpers.GetNamespace(e.Value.ToFSharpOcelEvent())).Distinct();
            var nsTree = OcelHelpers.NamespaceTree(new[] { '.' }, namespaces.Where(n => n.HasValue()).Select(n => n.Value));
            var highLevelNs = nsTree.Item2.Select(x => x.Item1); // TODO: Should be improved when FE can handle nested checkboxes
            return new LogInfo
            {
                ObjectTypes = log.ObjectTypes.Order(),
                Namespaces = highLevelNs,
                FirstEventTimestamp = log.OrderedEvents.FirstOrDefault().Value.Timestamp.ToString("yyyy/MM/dd"),
                LastEventTimestamp = log.OrderedEvents.LastOrDefault().Value.Timestamp.ToString("yyyy/MM/dd")
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
		public GraphLayout ComputeLayoutWithModel(string projectName, [FromBody] (pm4net.Types.DirectedGraph<Node<NodeInfo>, Edge<EdgeInfo>>, GraphLayoutOptions) modelAndOptions)
        {
	        var log = GetProjectLog(projectName);
	        return _graphLayoutService.ComputeGraphLayout(projectName, log, modelAndOptions.Item1,
		        modelAndOptions.Item2.MergeEdgesOfSameType, modelAndOptions.Item2.FixUnforeseenEdges,
		        modelAndOptions.Item2.NodeSeparation, modelAndOptions.Item2.RankSeparation);
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

        [HttpPost]
        [Route("discoverOcDfgAndGenerateMsaglSvg")]
        public string DiscoverOcDfgAndGenerateMsaglSvg(string projectName, bool groupByNamespace, [FromBody] OcDfgOptions options)
        {
            var ocDfg = DiscoverOriginalOcDfg(projectName, options);
            return pm4net.Visualization.Ocel.Msagl.OcDfg2Msagl(ocDfg, groupByNamespace);
        }

        [HttpPost]
        [Route("getTracesForObjectType")]
		public IEnumerable<(OcelObject, IEnumerable<(string, OcelEvent)>)> GetTracesForObjectType(string projectName, string objectType, [FromBody] OcDfgOptions options)
		{
            if (!string.IsNullOrWhiteSpace(projectName) && !string.IsNullOrWhiteSpace(objectType))
            {
                var log = GetProjectLog(projectName);
                var flattened = OcelHelpers.Flatten(log.ToFSharpOcelLog(), objectType);

                var namespaceFilter = ListTree<string>.NewNode("", options.IncludedNamespaces.Select(x => ListTree<string>.NewNode(x, new FSharpList<ListTree<string>>(ListTree<string>.NewNode("*", FSharpList<ListTree<string>>.Empty), FSharpList<ListTree<string>>.Empty))).ToFSharpList());
                TimeframeFilter? timeFrameFilter = null;
                if (options.DtoFrom != null && options.DtoTo != null)
                {
                    timeFrameFilter = new TimeframeFilter(options.DtoFrom.Value.StartOfDay(), options.DtoTo.Value.EndOfDay(), options.KeepCases.ToPm4Net());
                }

                var traces = OcelDfg.GetTracesForSingleType(
                    new OcDfgFilter(options.MinimumEvents, options.MinimumOccurrence, options.MinimumSuccessions, 
                        timeFrameFilter != null ? FSharpOption<TimeframeFilter>.Some(timeFrameFilter) : FSharpOption<TimeframeFilter>.None, 
                        options.IncludedLogLevels.Select(x => x.ToFSharpLogLevel()).ToFSharpList(), 
                        namespaceFilter), flattened);

                return traces.Select(x => (x.Item1.FromFSharpOcelObject(), x.Item2.Select(e => (e.Item1, e.Item2.FromRegularOcelEvent(flattened)))));
            }

            return new List<(OcelObject, IEnumerable<(string, OcelEvent)>)>();
        }
    }
}
