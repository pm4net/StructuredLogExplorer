using OCEL.CSharp;

namespace Infrastructure.Extensions
{
    public static class OcelHelpersExtensions
    {
        private static OcelValue? TryGetValue(OcelEvent @event, string? prefix, string name)
        {
            if (!string.IsNullOrWhiteSpace(prefix))
            {
                if (@event.VMap.TryGetValue($"{prefix}{name}", out var val))
                {
                    return val;
                }
            }

            return null;
        }

        private static OcelValue? TryGetValueWithPrefixes(OcelEvent @event, IEnumerable<string> prefixes, string name, OcelLog log)
        {
            foreach (var prefix in prefixes)
            {
                if (log.GlobalAttributes.TryGetValue(prefix, out var pf) && pf is OcelString pfStr)
                {
                    var val = TryGetValue(@event, pfStr.Value, name);
                    if (val != null) return val;
                }
            }

            // Try to obtain value with default prefix, otherwise give up
            var defaultVal = TryGetValue(@event, "pm4net_", name);
            return defaultVal;
        }

        public static OcelValue? Namespace(this OcelEvent @event, OcelLog log)
        {
            return TryGetValueWithPrefixes(@event, new List<string> { "Serilog.Sinks.OCEL_Prefix", "Serilog.Enrichers.CallerInfo_Prefix" }, "Namespace", log);
        }

        public static OcelValue? SourceFile(this OcelEvent @event, OcelLog log)
        {
            return TryGetValueWithPrefixes(@event, new List<string> { "Serilog.Sinks.OCEL_Prefix", "Serilog.Enrichers.CallerInfo_Prefix" }, "SourceFile", log);
        }

        public static OcelValue? LineNumber(this OcelEvent @event, OcelLog log)
        {
            return TryGetValueWithPrefixes(@event, new List<string> { "Serilog.Sinks.OCEL_Prefix", "Serilog.Enrichers.CallerInfo_Prefix" }, "LineNumber", log);
        }
    }
}
