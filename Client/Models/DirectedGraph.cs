using System.Runtime.Serialization;
using Newtonsoft.Json;
using NJsonSchema.Converters;

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
    }

    // https://github.com/RicoSuter/NJsonSchema/wiki/Inheritance
    [JsonConverter(typeof(JsonInheritanceConverter), "discriminator")]
    [KnownType(typeof(StartNode))]
    [KnownType(typeof(EndNode))]
    [KnownType(typeof(EventNode))]
    public abstract class Node { }
    
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
    }
}
