using Infrastructure.Extensions;
using OCEL.CSharp;
using pm4net.Types;

namespace Infrastructure.Models
{
	public class NodeInfo
	{
		public int Frequency { get; set; }

		public string? Namespace { get; set; }

		public LogLevel? LogLevel { get; set; }

        public IDictionary<string, OcelValue> Attributes { get; set; } = new Dictionary<string, OcelValue>();

        public IEnumerable<OcelObject> Objects { get; set; } = new List<OcelObject>();
    }

	public static class NodeInfoExtensions
	{
		public static NodeInfo FromFSharpNodeInfo(this pm4net.Types.NodeInfo nodeInfo)
		{
			return new NodeInfo
			{
				Frequency = nodeInfo.Frequency,
				Namespace = nodeInfo.Namespace.TryGetValue(),
				LogLevel = nodeInfo.Level.TryGetValue()?.FromFSharpLogLevel(),
				Attributes = nodeInfo.Attributes.ToDictionary(x => x.Key, x => x.Value.FromFSharpOcelValue()),
				Objects = nodeInfo.Objects.Select(o => o.FromFSharpOcelObject())
			};
		}
	}
}
