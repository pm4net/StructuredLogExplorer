using LiteDB;
using Newtonsoft.Json;

namespace Infrastructure.Models
{
	public class NodeCalculation
	{
		public NodeCalculation()
		{
		}
		
		public NodeCalculation(string nodeId, IEnumerable<string> textWrap, OutputTypes.Size size, NodeType nodeType)
		{
			NodeId = nodeId;
			TextWrap = textWrap;
			Size = size;
			NodeType = nodeType;
		}

		[BsonId]
		[JsonIgnore]
		public int Id { get; set; }

		public string NodeId { get; set; }

		public IEnumerable<string> TextWrap { get; set; }

		public OutputTypes.Size Size { get; set; }

		public NodeType NodeType { get; set; }
	}
}
