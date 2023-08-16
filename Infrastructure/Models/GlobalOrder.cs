﻿using Microsoft.FSharp.Collections;

namespace Infrastructure.Models
{
	public class GlobalOrder
	{
		public GlobalOrder() { }

		public GlobalOrder(pm4net.Types.DirectedGraph<Tuple<int, OutputTypes.SequenceNode>> globalOrderGraph, DateTime lastUpdated)
		{
			GlobalOrderGraph = globalOrderGraph;
			LastUpdated = lastUpdated;
		}

		public pm4net.Types.DirectedGraph<Tuple<int, OutputTypes.SequenceNode>> GlobalOrderGraph { get; set; } = new(FSharpList<Tuple<int, OutputTypes.SequenceNode>>.Empty, FSharpList<Tuple<Tuple<int, OutputTypes.SequenceNode>, Tuple<int, OutputTypes.SequenceNode>>>.Empty);

		public DateTime LastUpdated { get; set; }
	}
}
