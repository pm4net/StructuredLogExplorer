using OCEL.CSharp;
using OcelLog = OCEL.Types.OcelLog;

namespace Infrastructure.Models
{
	public class OcelEvent
	{
		public string Activity { get; set; } = string.Empty;

		public DateTimeOffset Timestamp { get; set; }

		public IDictionary<string, OcelObject> OMap { get; set; } = new Dictionary<string, OcelObject>();

		public IDictionary<string, OcelValue> VMap { get; set; } = new Dictionary<string, OcelValue>();
	}

	public static class OcelEventExtensions
	{
		public static OcelEvent FromRegularOcelEvent(this OCEL.Types.OcelEvent e, OcelLog log)
		{
			return new OcelEvent
			{
				Activity = e.Activity,
				Timestamp = e.Timestamp,
				OMap = e.OMap.ToDictionary(x => x, x =>
				{
					var obj = log.Objects[x];
					return new OcelObject(obj.Type, obj.OvMap.ToDictionary(x => x.Key, x => x.Value.FromFSharpOcelValue()));
				}),
				VMap = e.VMap.ToDictionary(x => x.Key, x => x.Value.FromFSharpOcelValue())
			};
		}
	}
}
