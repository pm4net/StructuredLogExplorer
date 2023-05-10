using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using Infrastructure.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using pm4net.Types;

namespace StructuredLogExplorer.Models.ControllerOptions
{
    public class OcDfgOptions
    {
        public int MinimumEvents { get; set; } = 0;

        public int MinimumOccurrence { get; set; } = 0;

        public int MinimumSuccessions { get; set; } = 0;

        public string? DateFrom { get; set; } // dd/MM/yyyy

        [JsonIgnore]
        public DateTimeOffset? DtoFrom => !string.IsNullOrWhiteSpace(DateFrom) ? DateTimeOffset.ParseExact(DateFrom, "dd/MM/yyyy", CultureInfo.InvariantCulture) : null;

        public string? DateTo { get; set; } // dd/MM/yyyy

        [JsonIgnore]
        public DateTimeOffset? DtoTo => !string.IsNullOrWhiteSpace(DateTo) ? DateTimeOffset.ParseExact(DateTo, "dd/MM/yyyy", CultureInfo.InvariantCulture) : null;

        public KeepCases KeepCases { get; set; }

        public IEnumerable<string> IncludedTypes { get; set; } = Enumerable.Empty<string>();
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum KeepCases
    {
        [EnumMember(Value = "contained")]
        ContainedInTimeFrame,
        [EnumMember(Value = "intersecting")]
        IntersectingTimeFrame,
        [EnumMember(Value = "started")]
        StartedInTimeFrame,
        [EnumMember(Value = "completed")]
        CompletedInTimeFrame,
        [EnumMember(Value = "trim")]
        TrimToTimeFrame
    }

    public static class KeepCasesExtension
    {
        public static KeepCases FromPm4Net(this pm4net.Types.KeepCases keepCases)
        {
            return (KeepCases) keepCases.Tag;
        }

        public static pm4net.Types.KeepCases ToPm4Net(this KeepCases keepCases)
        {
            return keepCases switch
            {
                KeepCases.ContainedInTimeFrame => pm4net.Types.KeepCases.ContainedInTimeframe,
                KeepCases.IntersectingTimeFrame => pm4net.Types.KeepCases.IntersectingTimeframe,
                KeepCases.StartedInTimeFrame => pm4net.Types.KeepCases.StartedInTimeframe,
                KeepCases.CompletedInTimeFrame => pm4net.Types.KeepCases.CompletedInTimeframe,
                KeepCases.TrimToTimeFrame => pm4net.Types.KeepCases.TrimToTimeframe,
                _ => pm4net.Types.KeepCases.ContainedInTimeframe
            };
        }
    }
}
