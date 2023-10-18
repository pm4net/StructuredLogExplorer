using FSharpx;

namespace Infrastructure.Models
{
	public class GlobalRanking
	{
		public GlobalRanking() { }
		
		public GlobalRanking(DirectedGraph<Tuple<string, int>, int> globalRankGraph, IEnumerable<IEnumerable<Tuple<pm4net.Types.GraphLayout.SequenceElement<string>, int>>> skeleton, IEnumerable<ISet<string>> components, DateTime lastUpdated)
		{
			GlobalRankGraph = globalRankGraph;
			Skeleton = skeleton;
			Components = components;
			LastUpdated = lastUpdated;
		}

		public DirectedGraph<Tuple<string, int>, int> GlobalRankGraph { get; set; } = new();

		public IEnumerable<IEnumerable<Tuple<pm4net.Types.GraphLayout.SequenceElement<string>, int>>> Skeleton { get; set; } = new List<IEnumerable<Tuple<pm4net.Types.GraphLayout.SequenceElement<string>, int>>>();

		public IEnumerable<ISet<string>> Components { get; set; } = new List<ISet<string>>();

		public DateTime LastUpdated { get; set; }
	}

	public static class GlobalRankingExtensions
	{
		public static GlobalRanking FromFSharpGlobalRanking(this pm4net.Types.GraphLayout.GlobalRanking globalRanking)
		{
			return new GlobalRanking
			{
				GlobalRankGraph = globalRanking.Graph.FromFSharpDirectedGraph(),
				Skeleton = globalRanking.Skeleton,
				Components = globalRanking.Components.Select(x => x.ToHashSet()),
				LastUpdated = DateTime.Now
			};
		}

		public static pm4net.Types.GraphLayout.GlobalRanking ToFSharpGlobalRanking(this GlobalRanking globalRanking)
		{
			return new pm4net.Types.GraphLayout.GlobalRanking(
				globalRanking.GlobalRankGraph.ToFSharpDirectedGraph(),
				globalRanking.Skeleton.Select(x => x.ToFSharpList()).ToFSharpList(),
				globalRanking.Components.Select(x => x.ToFSharpSet()).ToFSharpList());
		}
	}
}
