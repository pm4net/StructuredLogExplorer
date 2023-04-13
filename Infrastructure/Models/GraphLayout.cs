using System.Runtime.Serialization;
using Infrastructure.Extensions;
using Newtonsoft.Json;
using NJsonSchema.Converters;
using pm4net.Types;

namespace Infrastructure.Models
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

	public class Start : NodeType
	{
		public string ObjectType { get; set; } = string.Empty;
	}

	public class End : NodeType
	{
		public string ObjectType { get; set; } = string.Empty;
	}

	public class Waypoints
	{
		public IEnumerable<OutputTypes.Coordinate> Coordinates { get; set; } = Enumerable.Empty<OutputTypes.Coordinate>();

		public IEnumerable<OutputTypes.Coordinate> CatmullRom { get; set; } = Enumerable.Empty<OutputTypes.Coordinate>();

		public IEnumerable<Tuple<OutputTypes.Coordinate, OutputTypes.Coordinate, OutputTypes.Coordinate, OutputTypes.Coordinate>> Bezier { get; set; } = new List<Tuple<OutputTypes.Coordinate, OutputTypes.Coordinate, OutputTypes.Coordinate, OutputTypes.Coordinate>>();
	}

	public class Edge
	{
		public string SourceId { get; set; } = string.Empty;

		public string TargetId { get; set; } = string.Empty;

		public Waypoints Waypoints { get; set; } = new ();

		public bool Downwards { get; set; }

		public int TotalWeight { get; set; }

		public IEnumerable<EdgeTypeInfo<EdgeInfo>> TypeInfos { get; set; } = Enumerable.Empty<EdgeTypeInfo<EdgeInfo>>();
	}

	public class EdgeTypeInfo<T>
	{
		public int Weight { get; set; }

		public string? Type { get; set; }

		public T? Info { get; set; }
	}

	public static class GraphLayoutExtensions
	{
		private static Node FromFSharpNode(this OutputTypes.Node<pm4net.Types.NodeInfo> node)
		{
			var objType = string.Empty;
			NodeType nodeType = new Event();
			if (node.NodeType.IsEvent)
			{
				nodeType = new Event();
			}
			else if (node.NodeType.TryStart(ref objType))
			{
				nodeType = new Start
				{
					ObjectType = objType
				};
			}
			else if (node.NodeType.TryEnd(ref objType))
			{
				nodeType = new End
				{
					ObjectType = objType
				};
			}

			return new Node
			{
				Id = node.Id,
				Text = node.Text,
				NodeType = nodeType,
				Position = node.Position,
				Size = node.Size,
				Rank = node.Rank,
				NodeInfo = node.Info.TryGetValue()?.FromFSharpNodeInfo()
			};
		}

		private static Edge FromFSharpEdge(this OutputTypes.Edge<EdgeInfo> edge)
		{
			return new Edge
			{
				SourceId = edge.SourceId,
				TargetId = edge.TargetId,
				Waypoints = new Waypoints
				{
					Coordinates = edge.Waypoints.Coordinates,
					CatmullRom = edge.Waypoints.CatmullRom,
					Bezier = edge.Waypoints.Bezier
				},
				Downwards = edge.Downwards,
				TotalWeight = edge.TotalWeight,
				TypeInfos = edge.TypeInfos.Select(x => new EdgeTypeInfo<EdgeInfo>
				{
					Weight = x.Weight,
					Type = x.Type.IsSome() ? x.Type.Value : null,
					Info = x.Info.IsSome() ? x.Info.Value : null
				})
			};
		}

		public static GraphLayout FromFSharpGraphLayout(this OutputTypes.GraphLayout<pm4net.Types.NodeInfo, EdgeInfo> graphLayout)
		{
			return new GraphLayout
			{
				Nodes = graphLayout.Nodes.Select(FromFSharpNode),
				Edges = graphLayout.Edges.Select(FromFSharpEdge)
			};
		}
	}
}
