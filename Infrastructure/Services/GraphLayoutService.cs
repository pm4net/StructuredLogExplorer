using FSharpx;
using Infrastructure.Constants;
using Infrastructure.Interfaces;
using Infrastructure.Models;
using LiteDB;
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

			var lineWrapFunc = new Func<string, IEnumerable<string>>(str => nodeCalculations[str].Item1);
			var nodeSizeFunc = new Func<OutputTypes.Node<NodeInfo>, OutputTypes.Size>(n => nodeCalculations[n.Id].Item2);

			if (fixUnforeseenEdges)
			{
				var globalRanking = GetGlobalRanking(projectName);
				if (globalRanking is null)
				{
					var traces = OcelHelpers.AllTracesOfLog(log.ToFSharpOcelLog());
					globalRanking = ProcessGraphLayout.DefaultCustomMeasurements.ComputeGlobalRanking(traces);
					SaveGlobalRanking(projectName, globalRanking);
				}

				var discoveredGraph = ProcessGraphLayout.DefaultCustomMeasurements.ComputeDiscoveredGraph(globalRanking, model, mergeEdges);
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
				
			}

			return null;
		}

		/// <summary>
		/// Save pre-calculated node sizes to the database, overwriting any existing values for duplicate keys.
		/// </summary>
		/// <param name="projectName">The name of the project.</param>
		/// <param name="nodes">The dictionary of node sizes.</param>
		public void SaveNodeCalculations(string projectName, IDictionary<string, (IEnumerable<string>, OutputTypes.Size)> nodes)
		{
			var db = _projectService.GetProjectDatabase(projectName);
			var coll = db.GetCollection<NodeCalculation>(Identifiers.NodeCalculations);

			// TODO: Handle start and end nodes properly, maybe by returning both id and display name when getting nodes

			foreach (var node in nodes)
			{
				var nodeCalc = new NodeCalculation(node.Key, node.Value.Item1, node.Value.Item2);
				if (!coll.Update(nodeCalc))
				{
					coll.Insert(nodeCalc);
				}
			}
		}

		/// <summary>
		/// Get the pre-calculated node sizes from the database.
		/// </summary>
		/// <param name="projectName">The name of the project.</param>
		private IDictionary<string, (IEnumerable<string>, OutputTypes.Size)> GetNodeCalculations(string projectName)
		{
			var db = _projectService.GetProjectDatabase(projectName);
			var sizeColl = db.GetCollection<NodeCalculation>(Identifiers.NodeCalculations);
			var items = sizeColl.FindAll().ToList();
			return items.ToDictionary(x => x.Id, x => (x.TextWrap, x.Size));
		}
		
		private void SaveGlobalRanking(string projectName, OutputTypes.GlobalRanking ranking)
		{
			var db = _projectService.GetProjectDatabase(projectName);
			var coll = db.GetCollection<OutputTypes.GlobalRanking>(Identifiers.GlobalRanking);
			coll.DeleteAll();
			coll.Insert(ranking);
		}

		private OutputTypes.GlobalRanking? GetGlobalRanking(string projectName)
		{
			var db = _projectService.GetProjectDatabase(projectName);
			var coll = db.GetCollection<OutputTypes.GlobalRanking>(Identifiers.GlobalRanking);
			return coll.FindOne(_ => true);
		}

		private class NodeCalculation
		{
			public NodeCalculation()
			{
			}

			public NodeCalculation(string id, IEnumerable<string> textWrap, OutputTypes.Size size)
			{
				Id = id;
				TextWrap = textWrap;
				Size = size;
			}

			[BsonId]
			public string Id { get; set; }

			public IEnumerable<string> TextWrap { get; set; }

			public OutputTypes.Size Size { get; set; }
		}
	}
}
