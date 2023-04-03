using Infrastructure.Models;

namespace StructuredLogExplorer.Models
{
	public class LogNode
	{
		public LogNode(string id, string displayName, NodeType nodeType)
		{
			Id = id;
			DisplayName = displayName;
			NodeType = nodeType;
		}

		public string Id { get; set; }

		public string DisplayName { get; set; }

		public NodeType NodeType { get; set; }
	}
}
