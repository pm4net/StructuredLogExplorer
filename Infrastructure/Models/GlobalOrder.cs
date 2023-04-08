using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Models
{
	public class GlobalOrder
	{
		public GlobalOrder() { }

		public GlobalOrder(GraphTypes.DirectedGraph<Tuple<int, OutputTypes.SequenceNode>> globalOrderGraph, DateTime lastUpdated)
		{
			GlobalOrderGraph = globalOrderGraph;
			LastUpdated = lastUpdated;
		}

		public GraphTypes.DirectedGraph<Tuple<int, OutputTypes.SequenceNode>> GlobalOrderGraph { get; set; }

		public DateTime LastUpdated { get; set; }
	}
}
