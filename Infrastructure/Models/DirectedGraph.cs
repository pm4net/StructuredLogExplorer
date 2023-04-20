using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FSharpx;
using LiteDB;

namespace Infrastructure.Models
{
	public class DirectedGraph<TNode, TEdge>
	{
		public DirectedGraph(IEnumerable<TNode> nodes, IEnumerable<Tuple<TNode, TNode, TEdge>> edges)
		{
			Nodes = nodes;
			Edges = edges;
		}

		public DirectedGraph()
		{
		}

		public IEnumerable<TNode> Nodes { get; set; } = new List<TNode>();

		public IEnumerable<Tuple<TNode, TNode, TEdge>> Edges { get; set; } = new List<Tuple<TNode, TNode, TEdge>>(); 
	}

	public static class DirectedGraphExtensions
	{
		public static DirectedGraph<TNode, TEdge> FromFSharpDirectedGraph<TNode, TEdge>(this GraphTypes.DirectedGraph<TNode, TEdge> graph)
		{
			return new DirectedGraph<TNode, TEdge>
			{
				Nodes = graph.Nodes,
				Edges = graph.Edges
			};
		}

		public static GraphTypes.DirectedGraph<TNode, TEdge> ToFSharpDirectedGraph<TNode, TEdge>(this DirectedGraph<TNode, TEdge> graph)
		{
			return new GraphTypes.DirectedGraph<TNode, TEdge>(graph.Nodes.ToFSharpList(), graph.Edges.ToFSharpList());
		}
	}
}
