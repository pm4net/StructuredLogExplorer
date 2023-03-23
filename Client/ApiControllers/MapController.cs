﻿using Infrastructure.Interfaces;
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
using StructuredLogExplorer.Models.ControllerOptions;

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
            return new LogInfo { ObjectTypes = log.ObjectTypes.Order() };
        }

        [HttpPost]
        [Route("discoverOcDfg")]
        [OutputCache] // TODO: Invalidate in FileController when new log files are imported
        public DirectedGraph<Node, Edge> DiscoverObjectCentricDirectlyFollowsGraph(string projectName, [FromBody] OcDfgOptions options)
        {
            var log = GetProjectLog(projectName);
            return OcelDfg.Discover(options.MinimumEvents, options.MinimumOccurrence, options.MinimumSuccessions, options.IncludedTypes, log).FromFSharpGraph();
        }

        [HttpPost]
        [Route("discoverOcDfgAndApplyStableGraphLayout")]
        [OutputCache] // TODO: Invalidate in FileController when new log files are imported
        public GraphLayout DiscoverOcDfgAndApplyStableGraphLayout(string projectName, [FromBody] OcDfgLayoutOptions options)
        {
            // Discover object-centric directly follows graph
            var log = GetProjectLog(projectName);
            var ocDfg = OcelDfg.Discover(options.OcDfgOptions.MinimumEvents, options.OcDfgOptions.MinimumOccurrence, options.OcDfgOptions.MinimumSuccessions, options.OcDfgOptions.IncludedTypes, log);

            // Compute the final graph layout based on discovered model
            var (rankGraph, skeleton, components) = StableGraphLayout.ComputeRankGraph(log);
            var layout = StableGraphLayout.ComputeGlobalOrder(rankGraph, skeleton, components, ocDfg, 
	            options.LayoutOptions.MergeEdgesOfSameType, options.LayoutOptions.MaxCharsPerLine, options.LayoutOptions.NodeSeparation, 
	            options.LayoutOptions.RankSeparation, options.LayoutOptions.EdgeSeparation);
            return layout;
        }

        [HttpPost]
        [Route("discoverOcDfgAndDot")]
        [OutputCache] // TODO: Invalidate in FileController when new log files are imported
        public string DiscoverOcDfgAndGenerateDot(string projectName, bool groupByNamespace, [FromBody] OcDfgOptions options)
        {
            var log = GetProjectLog(projectName);
            var ocDfg = OcelDfg.Discover(options.MinimumEvents, options.MinimumOccurrence, options.MinimumSuccessions, options.IncludedTypes, log);
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
