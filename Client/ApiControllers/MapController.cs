using FSharpx;
using Infrastructure.Interfaces;
using Infrastructure.Models;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using pm4net.Algorithms.Discovery.Ocel;
using OCEL.CSharp;
using pm4net.Types.Trees;
using pm4net.Utilities;
using pm4net.Types;
using StructuredLogExplorer.Models;
using StructuredLogExplorer.Models.ControllerOptions;

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

        [HttpGet]
        [Route("getLogInfo")]
        [OutputCache] // TODO: Invalidate in FileController when new log files are imported
        public LogInfo GetLogInfo(string projectName)
        {
            var log = GetProjectLog(projectName);
            return new LogInfo { ObjectTypes = log.ObjectTypes.Order() };
        }

        [HttpPost]
        [Route("discoverOcDfg")]
        [OutputCache] // TODO: Invalidate in FileController when new log files are imported
        public GraphTypes.DirectedGraph<InputTypes.Node<NodeInfo>, InputTypes.Edge<EdgeInfo>> DiscoverOcDfg(string projectName, [FromBody] OcDfgOptions options)
        {
            var log = GetProjectLog(projectName);
            return OcelDfg.Discover(options.MinimumEvents, options.MinimumOccurrence, options.MinimumSuccessions, options.IncludedTypes, log);
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
        [OutputCache] // TODO: Invalidate in FileController when new log files are imported
		public GraphLayout ComputeLayoutWithModel(string projectName, [FromBody] (GraphTypes.DirectedGraph<InputTypes.Node<NodeInfo>, InputTypes.Edge<EdgeInfo>>, GraphLayoutOptions) modelAndOptions)
        {
	        var log = GetProjectLog(projectName);
	        return _graphLayoutService.ComputeGraphLayout(projectName, log, modelAndOptions.Item1,
		        modelAndOptions.Item2.MergeEdgesOfSameType, modelAndOptions.Item2.FixUnforeseenEdges,
		        modelAndOptions.Item2.NodeSeparation, modelAndOptions.Item2.RankSeparation,
		        modelAndOptions.Item2.EdgeSeparation);
        }

		[HttpPost]
		[Route("computeLayout")]
		[OutputCache] // TODO: Invalidate in FileController when new log files are imported
		public GraphLayout ComputeLayout(string projectName, [FromBody] OcDfgLayoutOptions options)
		{
			var model = DiscoverOcDfg(projectName, options.OcDfgOptions);
			return ComputeLayoutWithModel(projectName, (model, options.LayoutOptions));
		}

		[HttpPost]
        [Route("discoverOcDfgAndApplyStableGraphLayout")]
        [OutputCache] // TODO: Invalidate in FileController when new log files are imported
        public GraphLayout DiscoverOcDfgAndApplyStableGraphLayout(string projectName, [FromBody] OcDfgLayoutOptions options)
        {
            // Discover object-centric directly follows graph
            var log = GetProjectLog(projectName);
            var ocDfg = OcelDfg.Discover(options.OcDfgOptions.MinimumEvents, options.OcDfgOptions.MinimumOccurrence, options.OcDfgOptions.MinimumSuccessions, options.OcDfgOptions.IncludedTypes, log);

            var lineWrapFunc = new Func<string, IEnumerable<string>>(str => new List<string> { str }); // TODO: Calculate properly
            var nodeSizeFunc = new Func<OutputTypes.Node<NodeInfo>, OutputTypes.Size>(n => new OutputTypes.Size(n.Text.MaxBy(x => x.Length)?.Length ?? 0, n.Text.Count())); // TODO: Calculate properly

            var traces = OcelHelpers.AllTracesOfLog(log.ToFSharpOcelLog());
            if (options.LayoutOptions.FixUnforeseenEdges)
            {
	            var globalRanking = ProcessGraphLayout.DefaultCustomMeasurements.ComputeGlobalRanking(traces); // TODO: Cacheable
	            /*if (options.LayoutOptions.UseCustomMeasurements)
	            {
		            var discoveredGraph = ProcessGraphLayout.DefaultCustomMeasurements.ComputeDiscoveredGraph(globalRanking, ocDfg, options.LayoutOptions.MergeEdgesOfSameType); // TODO: Cacheable
		            return ProcessGraphLayout.DefaultCustomMeasurements.ComputeNodePositions(
			            FSharpFunc.FromFunc(lineWrapFunc), FSharpFunc.FromFunc(nodeSizeFunc), discoveredGraph, ocDfg,
			            options.LayoutOptions.NodeSeparation, options.LayoutOptions.RankSeparation,
			            options.LayoutOptions.EdgeSeparation).FromFSharpGraphLayout();
	            }*/

	            return ProcessGraphLayout.Default.ComputeLayout(globalRanking, ocDfg,
		            options.LayoutOptions.MergeEdgesOfSameType, 30,
		            options.LayoutOptions.NodeSeparation, options.LayoutOptions.RankSeparation,
		            options.LayoutOptions.EdgeSeparation).FromFSharpGraphLayout();
            }
            else
            {
				var (globalOrder, globalRanking) = ProcessGraphLayout.FastDefault.ComputeGlobalOrder(traces); // TODO: Cacheable
				/*if (options.LayoutOptions.UseCustomMeasurements)
				{
					return ProcessGraphLayout.FastCustomMeasurements.ComputeLayout(FSharpFunc.FromFunc(lineWrapFunc), FSharpFunc.FromFunc(nodeSizeFunc), globalOrder,
						globalRanking, ocDfg, options.LayoutOptions.NodeSeparation,
						options.LayoutOptions.RankSeparation, options.LayoutOptions.EdgeSeparation,
						options.LayoutOptions.MergeEdgesOfSameType).FromFSharpGraphLayout();
				}*/

				return ProcessGraphLayout.FastDefault.ComputeLayout(globalOrder, globalRanking, ocDfg,
					options.LayoutOptions.MergeEdgesOfSameType, 30,
					options.LayoutOptions.NodeSeparation, options.LayoutOptions.RankSeparation,
					options.LayoutOptions.EdgeSeparation).FromFSharpGraphLayout();
			}
        }

        [HttpPost]
        [Route("discoverOcDfgAndDot")]
        [OutputCache] // TODO: Invalidate in FileController when new log files are imported
        public string DiscoverOcDfgAndGenerateDot(string projectName, bool groupByNamespace, [FromBody] OcDfgOptions options)
        {
	        var ocDfg = DiscoverOcDfg(projectName, options);
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
            return OcelHelpers.NamespaceTree(new[] {'.'}, namespaces.Select(n => n.Value)); // TODO: Check if null first
        }
    }
}
