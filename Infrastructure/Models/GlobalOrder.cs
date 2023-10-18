using Microsoft.FSharp.Collections;
using pm4net.Types.GraphLayout;

namespace Infrastructure.Models
{
	public class GlobalOrder
	{
		public GlobalOrder() { }

		public GlobalOrder(pm4net.Types.DirectedGraph<Tuple<int, SequenceNode>> globalOrderGraph, DateTime lastUpdated)
		{
			GlobalOrderGraph = globalOrderGraph;
			LastUpdated = lastUpdated;
		}

		public pm4net.Types.DirectedGraph<Tuple<int, SequenceNode>> GlobalOrderGraph { get; set; } = new(FSharpList<Tuple<int, SequenceNode>>.Empty, FSharpList<Tuple<Tuple<int, SequenceNode>, Tuple<int, SequenceNode>>>.Empty);

		public DateTime LastUpdated { get; set; }
	}
}
