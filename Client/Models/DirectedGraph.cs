using System.Runtime.Serialization;
using Newtonsoft.Json;
using NJsonSchema.Converters;
using pm4net.Types.GraphLayout;
using static pm4net.Types.Graphs.Node;

namespace StructuredLogExplorer.Models
{
    public class DirectedGraph<TNode, TEdge>
    {
        public IEnumerable<TNode> Nodes { get; set; } = new List<TNode>();

        public IEnumerable<(TNode, TNode, TEdge)> Edges { get; set; } = new List<(TNode, TNode, TEdge)>();
    }

    public class Edge
    {
        public string Type { get; set; } = string.Empty;

        public EdgeStatistics Statistics { get; set; } = new();

        public IEnumerable<Coordinate> Waypoints { get; set; } = new List<Coordinate>();
    }

    // https://github.com/RicoSuter/NJsonSchema/wiki/Inheritance
    [JsonConverter(typeof(JsonInheritanceConverter), "discriminator")]
    [KnownType(typeof(StartNode))]
    [KnownType(typeof(EndNode))]
    [KnownType(typeof(EventNode))]
    public abstract class Node
    {
        public Coordinate? Coordinate { get; set; }
    }
    
    public class StartNode : Node
    {
        public string Type { get; set; } = string.Empty;
    }
    
    public class EndNode : Node
    {
        public string Type { get; set; } = string.Empty;
    }
    
    public class EventNode : Node
    {
        public string Name { get; set; } = string.Empty;

        public LogLevel Level { get; set; } = LogLevel.Unknown;

        public string Namespace { get; set; } = string.Empty;

        public NodeStatistics Statistics { get; set; } = new();
    }

    public class NodeStatistics
    {
        public int Frequency { get; set; }
    }

    public class EdgeStatistics
    {
        public int Frequency { get; set; }
    }

    public enum LogLevel
    {
        Verbose,
        Debug,
        Information,
        Warning,
        Error,
        Fatal,
        Unknown
    }

    /* --- Converter extensions --- */

    public static class DirectedGraphExtensions
    {
        private static Node FromFSharpNode(this pm4net.Types.Graphs.Node node)
        {
            if (node.IsStartNode)
            {
                var sn = (pm4net.Types.Graphs.Node.StartNode) node;
                return new StartNode { Type = sn.Type };
            }

            if (node.IsEndNode)
            {
                var en = (pm4net.Types.Graphs.Node.EndNode) node;
                return new EndNode { Type = en.Type };
            }

            var eventNode = (pm4net.Types.Graphs.Node.EventNode) node;
            return new EventNode
            {
                Name = eventNode.Item.Name,
                Level = (LogLevel) eventNode.Item.Level.Tag,
                Namespace = eventNode.Item.Namespace,
                Statistics = new NodeStatistics
                {
                    Frequency = eventNode.Item.Statistics.Frequency
                }
            };
        }

        private static Edge FromFSharpEdge(this pm4net.Types.Graphs.Edge edge)
        {
            return new Edge
            {
                Type = edge.Type,
                Statistics = new EdgeStatistics
                {
                    Frequency = edge.Statistics.Frequency
                }
            };
        }

        public static DirectedGraph<Node, Edge> FromFSharpGraph(this pm4net.Types.Graphs.DirectedGraph<pm4net.Types.Graphs.Node, pm4net.Types.Graphs.Edge> graph)
        {
            return new DirectedGraph<Node, Edge>
            {
                Nodes = graph.Nodes.Select(n => n.FromFSharpNode()),
                Edges = graph.Edges.Select(e => (e.Item1.FromFSharpNode(), e.Item2.FromFSharpNode(), e.Item3.FromFSharpEdge()))
            };
        }

        /// <summary>
        /// Enrich a directed graph of nodes and edges with positional information from a global order.
        /// </summary>
        public static DirectedGraph<Node, Edge> EnrichWithGlobalOrder(this DirectedGraph<Node, Edge> graph, GlobalOrder globalOrder)
        {
            graph.Nodes = graph.Nodes.Select(node =>
            {
                node.Coordinate = globalOrder.Nodes.FirstOrDefault(gn => gn.Name == GetNodeName(node))?.Position;
                return node;
            });

            graph.Edges = graph.Edges.Select(edge =>
            {
                var edgeNames = (GetNodeName(edge.Item1), GetNodeName(edge.Item2));
                var edgePath = globalOrder.EdgePaths.FirstOrDefault(ep => 
                    ep.Edge.Item1 == edgeNames.Item1 && ep.Edge.Item2 == edgeNames.Item2 ||
                    ep.Edge.Item1 == edgeNames.Item2 && ep.Edge.Item2 == edgeNames.Item1);

                edge.Item1.Coordinate = graph.Nodes.FirstOrDefault(n => GetNodeName(n) == edgeNames.Item1)?.Coordinate;
                edge.Item2.Coordinate = graph.Nodes.FirstOrDefault(n => GetNodeName(n) == edgeNames.Item2)?.Coordinate;
                if (edgePath is not null)
                {
                    edge.Item3.Waypoints = edgePath.Waypoints;
                }
                return edge;
            });

            return graph;

            string GetNodeName(Node node)
            {
                return node switch
                {
                    EventNode eventNode => eventNode.Name,
                    StartNode startNode => pm4net.Types.Constants.objectTypeStartNode + startNode.Type,
                    EndNode endNode => pm4net.Types.Constants.objectTypeEndNode + endNode.Type,
                    _ => throw new ArgumentOutOfRangeException(nameof(node))
                };
            }
        }
    }
}
