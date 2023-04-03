using FSharpx;
using static OutputTypes;

namespace Infrastructure.Models
{
	public class GlobalRanking
	{
		public GlobalRanking() { }

		public GlobalRanking(GraphTypes.DirectedGraph<Tuple<string, int>, int> globalRankGraph, IEnumerable<IEnumerable<Tuple<OutputTypes.SequenceElement<string>, int>>> skeleton, IEnumerable<ISet<string>> components)
		{
			GlobalRankGraph = globalRankGraph;
			Skeleton = skeleton;
			Components = components;
		}

		public GraphTypes.DirectedGraph<Tuple<string, int>, int> GlobalRankGraph { get; set; }

		public IEnumerable<IEnumerable<Tuple<OutputTypes.SequenceElement<string>, int>>> Skeleton { get; set; }

		public IEnumerable<ISet<string>> Components { get; set; }
	}

	public static class GlobalRankingExtensions
	{
		public static GlobalRanking FromFSharpGlobalRanking(this OutputTypes.GlobalRanking globalRanking)
		{
			return new GlobalRanking
			{
				GlobalRankGraph = globalRanking.Graph,
				Skeleton = globalRanking.Skeleton,
				Components = globalRanking.Components.Select(x => x.ToHashSet())
			};
		}

		public static OutputTypes.GlobalRanking ToFSharpGlobalRanking(this GlobalRanking globalRanking)
		{
			return new OutputTypes.GlobalRanking(
				globalRanking.GlobalRankGraph,
				globalRanking.Skeleton.Select(x => x.ToFSharpList()).ToFSharpList(),
				globalRanking.Components.Select(x => x.ToFSharpSet()).ToFSharpList());
		}
	}
}
