using FSharpx;
using Infrastructure.Constants;
using Infrastructure.Interfaces;
using Infrastructure.Models;
using OCEL.CSharp;
using pm4net.Types;
using pm4net.Utilities;

namespace Infrastructure.Services
{
	public class GraphLayoutService : IGraphLayoutService
	{
		private readonly IProjectService _projectService;

		public GraphLayoutService(IProjectService projectService)
		{
			_projectService = projectService;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="projectName"></param>
		/// <param name="log"></param>
		/// <param name="model"></param>
		/// <param name="mergeEdges"></param>
		/// <param name="fixUnforeseenEdges"></param>
		/// <param name="nodeSep"></param>
		/// <param name="rankSep"></param>
		/// <param name="edgeSep"></param>
		/// <returns></returns>
		public GraphLayout ComputeGraphLayout(
			string projectName, 
			OcelLog log, 
			GraphTypes.DirectedGraph<InputTypes.Node<NodeInfo>, InputTypes.Edge<EdgeInfo>> model, 
			bool mergeEdges, 
			bool fixUnforeseenEdges, 
			float nodeSep,
			float rankSep, 
			float edgeSep)
		{
			var nodeCalculations = GetNodeCalculations(projectName);
			if (nodeCalculations is null)
			{
				throw new ArgumentNullException(nameof(nodeCalculations), "Node calculations have not been made yet.");
			}

			var lineWrapFunc = new Func<OutputTypes.Node<NodeInfo>, IEnumerable<string>>(n => nodeCalculations.First(x => x.NodeId == n.Id).TextWrap);
			var nodeSizeFunc = new Func<OutputTypes.Node<NodeInfo>, OutputTypes.Size>(n => nodeCalculations.First(x => x.NodeId == n.Id).Size);

			var globalRanking = GetGlobalRanking(projectName);
			var projectDb = _projectService.GetProjectDatabase(projectName);
			var importedLogs = projectDb.GetCollection<LogFileInfo>(Identifiers.LogFilesInfo)?.FindAll() ?? new List<LogFileInfo>();
			if (globalRanking is null || importedLogs.Any(l => l.LastImported >= globalRanking?.LastUpdated))
			{
				var traces = OcelHelpers.AllTracesOfLog(log.ToFSharpOcelLog());
				globalRanking = ProcessGraphLayout.DefaultCustomMeasurements.ComputeGlobalRanking(traces).FromFSharpGlobalRanking();
				SaveGlobalRanking(projectName, globalRanking);
			}

			if (fixUnforeseenEdges)
			{
				var discoveredGraph = ProcessGraphLayout.DefaultCustomMeasurements.ComputeDiscoveredGraph(globalRanking.ToFSharpGlobalRanking(), model, mergeEdges);
				return ProcessGraphLayout.DefaultCustomMeasurements.ComputeNodePositions(
					FSharpFunc.FromFunc(lineWrapFunc), 
					FSharpFunc.FromFunc(nodeSizeFunc), 
					discoveredGraph, 
					model, 
					nodeSep,
					rankSep,
					edgeSep)
					.FromFSharpGraphLayout();
			}
			else
			{
				var fSharpGlobalRanking = globalRanking.ToFSharpGlobalRanking();
				var nsg = ProcessGraphLayout.FastCustomMeasurements.ComputeNodeSequenceGraph(fSharpGlobalRanking); // TODO: Save NSG in DB
				var globalOrder = ProcessGraphLayout.FastCustomMeasurements.ComputeGlobalOrder(fSharpGlobalRanking, nsg); // TODO: Save GO in DB
				return ProcessGraphLayout.FastCustomMeasurements.ComputeLayout(
					FSharpFunc.FromFunc(lineWrapFunc),
					FSharpFunc.FromFunc(nodeSizeFunc),
					globalOrder,
					fSharpGlobalRanking,
					model,
					nodeSep,
					rankSep,
					edgeSep,
					mergeEdges)
					.FromFSharpGraphLayout();
			}
		}

		/// <summary>
		/// Save pre-calculated node sizes to the database, overwriting any existing values for duplicate keys.
		/// </summary>
		/// <param name="projectName">The name of the project.</param>
		/// <param name="nodes">The dictionary of node sizes.</param>
		public void SaveNodeCalculations(string projectName, IEnumerable<NodeCalculation> nodes)
		{
			var db = _projectService.GetProjectDatabase(projectName);
			var coll = db.GetCollection<NodeCalculation>(Identifiers.NodeCalculations);
			coll.DeleteAll();
			coll.InsertBulk(nodes);
		}

		/// <summary>
		/// Get the pre-calculated node sizes from the database.
		/// </summary>
		/// <param name="projectName">The name of the project.</param>
		private IEnumerable<NodeCalculation> GetNodeCalculations(string projectName)
		{
			var db = _projectService.GetProjectDatabase(projectName);
			var sizeColl = db.GetCollection<NodeCalculation>(Identifiers.NodeCalculations);
			return sizeColl.FindAll();
		}
		
		/// <summary>
		/// Save a pre-calculated global ranking to the database.
		/// </summary>
		/// <param name="projectName"></param>
		/// <param name="ranking"></param>
		private void SaveGlobalRanking(string projectName, GlobalRanking ranking)
		{
			var db = _projectService.GetProjectDatabase(projectName);
			var coll = db.GetCollection<GlobalRanking>(Identifiers.GlobalRanking);
			coll.DeleteAll();
			coll.Insert(ranking);
		}

		/// <summary>
		/// Get the pre-calculated global ranking from the database.
		/// </summary>
		/// <param name="projectName"></param>
		/// <returns></returns>
		private GlobalRanking? GetGlobalRanking(string projectName)
		{
			var db = _projectService.GetProjectDatabase(projectName);
			var coll = db.GetCollection<GlobalRanking>(Identifiers.GlobalRanking);
			return coll.FindOne(_ => true);
		}
	}
}
