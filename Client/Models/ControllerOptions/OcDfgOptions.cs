using System.ComponentModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace StructuredLogExplorer.Models.ControllerOptions
{
    public class OcDfgOptions
    {
        public int MinimumEvents { get; set; } = 0;

        public int MinimumOccurrence { get; set; } = 0;

        public int MinimumSuccessions { get; set; } = 0;

        public string? DateFrom { get; set; } // dd/MM/yyyy

        public string? DateTo { get; set; } // dd/MM/yyyy

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
}
