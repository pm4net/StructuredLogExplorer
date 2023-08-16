using Infrastructure.Models;
using LiteDB;
using Newtonsoft.Json;
using static OutputTypes;

namespace StructuredLogExplorer.Mappers
{
	public static class LiteDbBsonMappers
	{
		public static Func<Infrastructure.Models.GlobalRanking, BsonValue> SerializeGlobalRanking => ranking => new BsonDocument
		{
			["_id"] = Guid.NewGuid().ToString(),
			["ranking"] = JsonConvert.SerializeObject(ranking.GlobalRankGraph),
			["skeleton"] = JsonConvert.SerializeObject(ranking.Skeleton),
			["components"] = BsonMapper.Global.Serialize(ranking.Components),
			["lastUpdated"] = BsonMapper.Global.Serialize(ranking.LastUpdated)
		};

		public static Func<BsonValue, Infrastructure.Models.GlobalRanking> DeserializeGlobalRanking => bson => new Infrastructure.Models.GlobalRanking
		{
			GlobalRankGraph = JsonConvert.DeserializeObject<DirectedGraph<Tuple<string, int>, int>>(bson["ranking"])!,
			Skeleton = JsonConvert.DeserializeObject<IEnumerable<IEnumerable<Tuple<SequenceElement<string>, int>>>>(bson["skeleton"])!,
			Components = BsonMapper.Global.Deserialize<IEnumerable<ISet<string>>>(bson["components"]),
			LastUpdated = BsonMapper.Global.Deserialize<DateTime>(bson["lastUpdated"])
		};

		public static Func<Infrastructure.Models.GlobalOrder, BsonValue> SerializeGlobalOrder => globalOrder => new BsonDocument
		{
			["_id"] = Guid.NewGuid().ToString(),
			["globalOrder"] = JsonConvert.SerializeObject(globalOrder.GlobalOrderGraph),
			["lastUpdated"] = BsonMapper.Global.Serialize(globalOrder.LastUpdated)
		};

		public static Func<BsonValue, Infrastructure.Models.GlobalOrder> DeserializeGlobalOrder => bson => new Infrastructure.Models.GlobalOrder
		{
			GlobalOrderGraph = JsonConvert.DeserializeObject<pm4net.Types.DirectedGraph<Tuple<int, SequenceNode>>>(bson["globalOrder"])!,
			LastUpdated = BsonMapper.Global.Deserialize<DateTime>(bson["lastUpdated"])
		};
	}
}
