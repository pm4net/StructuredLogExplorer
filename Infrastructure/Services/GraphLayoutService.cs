using FSharpx;
using Infrastructure.Constants;
using Infrastructure.Interfaces;
using Infrastructure.Models;
using OCEL.CSharp;
using pm4net.Types;
using pm4net.Utilities;
using NodeInfo = pm4net.Types.NodeInfo;

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
		/// <returns></returns>
		public GraphLayout ComputeGraphLayout(
			string projectName, 
			OcelLog log, 
			GraphTypes.DirectedGraph<InputTypes.Node<NodeInfo>, InputTypes.Edge<EdgeInfo>> model, 
			bool mergeEdges, 
			bool fixUnforeseenEdges, 
			float nodeSep,
			float rankSep)
		{
			var nodeCalculations = GetNodeCalculations(projectName);
			if (nodeCalculations is null)
			{
				throw new ArgumentNullException(nameof(nodeCalculations), "Node calculations have not been made yet.");
			}

			// Define custom functions
			var lineWrapFunc = new Func<OutputTypes.Node<NodeInfo>, IEnumerable<string>>(n => nodeCalculations.FirstOrDefault(x => x.NodeId == n.Id)?.TextWrap ?? new List<string>());
			var nodeSizeFunc = new Func<OutputTypes.Node<NodeInfo>, OutputTypes.Size?>(n => nodeCalculations.FirstOrDefault(x => x.NodeId == n.Id)?.Size);

			// Get traces in log
			var traces = OcelHelpers.AllTracesOfLog(log.ToFSharpOcelLog()).ToList();

			// Retrieve or generate global ranking
			var globalRanking = GetGlobalRanking(projectName);
			var projectDb = _projectService.GetProjectDatabase(projectName);
			var importedLogs = projectDb.GetCollection<LogFileInfo>(Identifiers.LogFilesInfo)?.FindAll().ToList() ?? new List<LogFileInfo>();
			if (globalRanking is null || importedLogs.Any(l => l.LastImported >= globalRanking?.LastUpdated))
			{
				globalRanking = ProcessGraphLayout.DefaultCustomMeasurements.ComputeGlobalRanking(traces).FromFSharpGlobalRanking();
				SaveGlobalRanking(projectName, globalRanking);
			}

			// Perform separate methods, depending on whether the fast method is used
			if (fixUnforeseenEdges)
			{
				var discoveredGraph = ProcessGraphLayout.DefaultCustomMeasurements.ComputeDiscoveredGraph(globalRanking.ToFSharpGlobalRanking(), model, mergeEdges);
				return ProcessGraphLayout.DefaultCustomMeasurements.ComputeNodePositions(
					FSharpFunc.FromFunc(lineWrapFunc), 
					FSharpFunc.FromFunc(nodeSizeFunc), 
					discoveredGraph, 
					model, 
					nodeSep,
					rankSep)
					.FromFSharpGraphLayout();
			}
			else
			{
				var globalOrder = GetGlobalOrder(projectName);
				if (globalOrder is null || importedLogs.Any(l => l.LastImported >= globalOrder?.LastUpdated))
				{
					globalOrder = new GlobalOrder(ProcessGraphLayout.FastCustomMeasurements.ComputeGlobalOrder(traces), DateTime.Now);
					SaveGlobalOrder(projectName, globalOrder);
				}

                return ProcessGraphLayout.FastCustomMeasurements.ComputeLayout(
					FSharpFunc.FromFunc(lineWrapFunc), 
					FSharpFunc.FromFunc(nodeSizeFunc),
					globalOrder.GlobalOrderGraph,
					globalRanking.ToFSharpGlobalRanking(),
					model,
					nodeSep,
					rankSep,
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
		public IEnumerable<NodeCalculation> GetNodeCalculations(string projectName)
		{
			var db = _projectService.GetProjectDatabase(projectName);
			var sizeColl = db.GetCollection<NodeCalculation>(Identifiers.NodeCalculations);
			return sizeColl.FindAll();
		}
		
		/// <summary>
		/// Save a pre-calculated global ranking to the database.
		/// </summary>
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
		private GlobalRanking? GetGlobalRanking(string projectName)
		{
			var db = _projectService.GetProjectDatabase(projectName);
			var coll = db.GetCollection<GlobalRanking>(Identifiers.GlobalRanking);
			return coll.FindOne(_ => true);
		}

		/// <summary>
		/// Save a pre-calculated global order to the database.
		/// </summary>
		private void SaveGlobalOrder(string projectName, GlobalOrder globalOrder)
		{
			var db = _projectService.GetProjectDatabase(projectName);
			var coll = db.GetCollection<GlobalOrder>(Identifiers.GlobalOrder);
			coll.DeleteAll();
			coll.Insert(globalOrder);
		}

		/// <summary>
		/// Get the pre-calculated global order from the database.
		/// </summary>
		private GlobalOrder? GetGlobalOrder(string projectName)
		{
			var db = _projectService.GetProjectDatabase(projectName);
			var coll = db.GetCollection<GlobalOrder>(Identifiers.GlobalOrder);
			return coll.FindOne(_ => true);
		}
	}
}
