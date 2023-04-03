using FSharpx;
using LiteDB;
using static OutputTypes;

namespace Infrastructure.Models
{
	public class GlobalRanking
	{
		public GlobalRanking() { }
		
		public GlobalRanking(DirectedGraph<Tuple<string, int>, int> globalRankGraph, IEnumerable<IEnumerable<Tuple<SequenceElement<string>, int>>> skeleton, IEnumerable<ISet<string>> components)
		{
			GlobalRankGraph = globalRankGraph;
			Skeleton = skeleton;
			Components = components;
		}

		// TODO: Custom deserializer/serializer
		public DirectedGraph<Tuple<string, int>, int> GlobalRankGraph { get; set; }

		// TODO: Custom deserializer/serializer
		public IEnumerable<IEnumerable<Tuple<SequenceElement<string>, int>>> Skeleton { get; set; }

		public IEnumerable<ISet<string>> Components { get; set; }
	}

	public static class GlobalRankingExtensions
	{
		public static GlobalRanking FromFSharpGlobalRanking(this OutputTypes.GlobalRanking globalRanking)
		{
			return new GlobalRanking
			{
				GlobalRankGraph = globalRanking.Graph.FromFSharpDirectedGraph(),
				Skeleton = globalRanking.Skeleton,
				Components = globalRanking.Components.Select(x => x.ToHashSet())
			};
		}

		public static OutputTypes.GlobalRanking ToFSharpGlobalRanking(this GlobalRanking globalRanking)
		{
			return new OutputTypes.GlobalRanking(
				globalRanking.GlobalRankGraph.ToFSharpDirectedGraph(),
				globalRanking.Skeleton.Select(x => x.ToFSharpList()).ToFSharpList(),
				globalRanking.Components.Select(x => x.ToFSharpSet()).ToFSharpList());
		}
	}
}
