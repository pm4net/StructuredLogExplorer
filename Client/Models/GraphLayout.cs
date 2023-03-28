using System.Runtime.Serialization;
using Microsoft.FSharp.Core;
using Newtonsoft.Json;
using NJsonSchema.Converters;
using pm4net.Types;
using StructuredLogExplorer.Extensions;

namespace StructuredLogExplorer.Models
{
	public class GraphLayout
	{
		public IEnumerable<Node> Nodes { get; init; } = Enumerable.Empty<Node>();

		public IEnumerable<Edge> Edges { get; init; } = Enumerable.Empty<Edge>();
	}

	public class Node
	{
		public string Id { get; set; } = string.Empty;

		public IEnumerable<string> Text { get; set; } = Enumerable.Empty<string>();

		public NodeType NodeType { get; set; } = new Event();

		public OutputTypes.Coordinate Position { get; set; } = new(0f, 0f);

		public OutputTypes.Size Size { get; set; } = new(0, 0);

		public int Rank { get; set; } = 0;

		public NodeInfo? NodeInfo { get; set; }
	}

	[KnownType(typeof(Event))]
	[KnownType(typeof(Start))]
	[KnownType(typeof(End))]
	[JsonConverter(typeof(JsonInheritanceConverter), "discriminator")]
	public abstract class NodeType { }
	public class Event : NodeType { }
	public class Start : NodeType { }
	public class End : NodeType { }

	public class Edge
	{
		public string SourceId { get; set; } = string.Empty;

		public string TargetId { get; set; } = string.Empty;

		public IEnumerable<OutputTypes.Coordinate> Waypoints { get; set; } = Enumerable.Empty<OutputTypes.Coordinate>();

		public bool Downwards { get; set; }

		public int TotalWeight { get; set; }

		public IEnumerable<OutputTypes.EdgeTypeInfo<EdgeInfo>> TypeInfos { get; set; } = Enumerable.Empty<OutputTypes.EdgeTypeInfo<EdgeInfo>>();
	}

	public static class GraphLayoutExtensions
	{
		private static Node FromFSharpNode(this OutputTypes.Node<NodeInfo> node)
		{
			NodeType nodeType = new Event();
			if (node.NodeType.IsEvent)
			{
				nodeType = new Event();
			}
			else if (node.NodeType.IsStart)
			{
				nodeType = new Start();
			}
			else if (node.NodeType.IsEnd)
			{
				nodeType = new End();
			}

			return new Node
			{
				Id = node.Id,
				Text = node.Text,
				NodeType = nodeType,
				Position = node.Position,
				Size = node.Size,
				Rank = node.Rank,
				NodeInfo = node.Info.IsSome() ? node.Info.Value : null,
			};
		}

		private static Edge FromFSharpEdge(this OutputTypes.Edge<EdgeInfo> edge)
		{
			return new Edge
			{
				SourceId = edge.SourceId,
				TargetId = edge.TargetId,
				Waypoints = edge.Waypoints,
				Downwards = edge.Downwards,
				TotalWeight = edge.TotalWeight,
				TypeInfos = edge.TypeInfos
			};
		}

		public static GraphLayout FromFSharpGraphLayout(this OutputTypes.GraphLayout<NodeInfo, EdgeInfo> graphLayout)
		{
			return new GraphLayout
			{
				Nodes = graphLayout.Nodes.Select(FromFSharpNode),
				Edges = graphLayout.Edges.Select(FromFSharpEdge)
			};
		}
	}
}
