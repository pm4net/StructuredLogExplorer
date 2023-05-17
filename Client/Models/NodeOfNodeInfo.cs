using System.Runtime.Serialization;
using FSharpx;
using Infrastructure.Models;
using Newtonsoft.Json;
using NJsonSchema.Converters;

namespace StructuredLogExplorer.Models
{
    [KnownType(typeof(StartNode))]
    [KnownType(typeof(EndNode))]
    [KnownType(typeof(EventNode))]
    [JsonConverter(typeof(JsonInheritanceConverter), "discriminator")]
    public abstract class NodeOfNodeInfo { }

    public class StartNode : NodeOfNodeInfo
    {
        public string Type { get; set; } = string.Empty;
    }

    public class EndNode : NodeOfNodeInfo
    {
        public string Type { get; set; } = string.Empty;
    }

    public class EventNode : NodeOfNodeInfo
    {
        public string Name { get; set; } = string.Empty;

        public NodeInfo? NodeInfo { get; set; }
    }

    public static class NodeOfNodeInfoExtensions
    {
        public static NodeOfNodeInfo FromFSharpNodeOfNodeInfo(this InputTypes.Node<pm4net.Types.NodeInfo> node)
        {
            var type = string.Empty;
            InputTypes.EventNode<pm4net.Types.NodeInfo>? nodeInfo = null;
            if (node.TryStartNode(ref type))
            {
                return new StartNode { Type = type };
            }

            if (node.TryEndNode(ref type))
            {
                return new EndNode { Type = type };
            }

            if (node.TryEventNode(ref nodeInfo))
            {
                return new EventNode
                {
                    Name = nodeInfo.Name,
                    NodeInfo = nodeInfo.Info.HasValue() ? nodeInfo.Info.Value.FromFSharpNodeInfo() : null
                };
            }

            throw new ArgumentOutOfRangeException(nameof(node), "Node not of any known type.");
        }
    }
}
