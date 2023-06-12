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

		public OutputTypes.Coordinate? Position { get; set; } = new(0f, 0f);

		public OutputTypes.Size? Size { get; set; } = new(0, 0);

		public int? Rank { get; set; } = 0;

		public NodeInfo? NodeInfo { get; set; }
	}

	// To allow NSwag to correctly generate subclasses (see https://github.com/RicoSuter/NSwag/issues/1766)
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

		public IEnumerable<Tuple<OutputTypes.Coordinate, OutputTypes.Coordinate, OutputTypes.Coordinate, OutputTypes.Coordinate>> CubicBezier { get; set; } = new List<Tuple<OutputTypes.Coordinate, OutputTypes.Coordinate, OutputTypes.Coordinate, OutputTypes.Coordinate>>();

        public IEnumerable<Tuple<OutputTypes.Coordinate, OutputTypes.Coordinate, OutputTypes.Coordinate>> QuadraticBezier { get; set; } = new List<Tuple<OutputTypes.Coordinate, OutputTypes.Coordinate, OutputTypes.Coordinate>>();
    }

	public class Edge
	{
		public string SourceId { get; set; } = string.Empty;

		public string TargetId { get; set; } = string.Empty;

        public Waypoints? Waypoints { get; set; }

        public bool? Downwards { get; set; }

        public bool Constrained { get; set; }

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
					CubicBezier = edge.Waypoints.CubicBezier,
					QuadraticBezier = edge.Waypoints.QuadraticBezier,
				},
				Downwards = edge.Downwards,
				Constrained = edge.Constrained,
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

        public static GraphLayout FromOcDfg(this GraphTypes.DirectedGraph<InputTypes.Node<pm4net.Types.NodeInfo>, InputTypes.Edge<EdgeInfo>> graph, IList<NodeCalculation> nodeCalculations)
        {
            return new GraphLayout
            {
				Nodes = graph.Nodes.Select(n =>
                {
                    NodeType nodeType = new Event();
					
                    string? objType = null;
                    InputTypes.EventNode<pm4net.Types.NodeInfo>? eventNode = null;
					NodeInfo? nodeInfo = null;

                    if (n.TryEventNode(ref eventNode))
                    {
                        nodeType = new Event();
                        nodeInfo = eventNode.Info.TryGetValue()?.FromFSharpNodeInfo();
                    }
                    else if (n.TryStartNode(ref objType))
                    {
                        nodeType = new Start
                        {
                            ObjectType = objType
                        };
                    }
					else if (n.TryEndNode(ref objType))
                    {
                        nodeType = new End
                        {
                            ObjectType = objType
                        };
                    }

                    var nodeId = GetNodeId(n);
                    return new Node
                    {
						Id = GetNodeId(n),
						Text = nodeCalculations.First(x => x.NodeId == nodeId).TextWrap,
						NodeType = nodeType,
						NodeInfo = nodeInfo,
						Size = nodeCalculations.First(x => x.NodeId == nodeId).Size
                    };
                }),
				Edges = graph.Edges.Select(e => new Edge
                {
                    SourceId = GetNodeId(e.Item1),
                    TargetId = GetNodeId(e.Item2),
                    TotalWeight = e.Item3.Weight,
                    TypeInfos = new List<EdgeTypeInfo<EdgeInfo>> {
                        new() {
                            Type = e.Item3.Type.TryGetValue(),
                            Weight = e.Item3.Weight,
                            Info = e.Item3.Info.TryGetValue()
                        }
                    }
                })
            };

            string GetNodeId<T>(InputTypes.Node<T> node)
            {
                string? objType = null;
                InputTypes.EventNode<T>? eventNode = null;
                if (node.TryEventNode(ref eventNode))
                {
                    return eventNode.Name;
                }

                if (node.TryStartNode(ref objType))
                {
                    return global::Constants.objectTypeStartNode + objType;
                }

                if (node.TryEndNode(ref objType))
                {
                    return global::Constants.objectTypeEndNode + objType;
                }

                throw new ArgumentOutOfRangeException(nameof(node), "Node has unknown type.");
            }
        }
 	}
}
