using Infrastructure.Extensions;
using pm4net.Types;

namespace Infrastructure.Models
{
	public class NodeInfo
	{
		public int Frequency { get; set; }

		public string? Namespace { get; set; }

		public LogLevel? LogLevel { get; set; }
	}

	public static class NodeInfoExtensions
	{
		public static NodeInfo FromFSharpNodeInfo(this pm4net.Types.NodeInfo nodeInfo)
		{
			return new NodeInfo
			{
				Frequency = nodeInfo.Frequency,
				Namespace = nodeInfo.Namespace.TryGetValue(),
				LogLevel = nodeInfo.Level.TryGetValue()?.FromFSharpLogLevel()
			};
		}
	}
}
