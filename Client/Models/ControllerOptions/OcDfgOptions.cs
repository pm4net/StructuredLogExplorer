using System.ComponentModel;

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

    public enum KeepCases
    {
        ContainedInTimeFrame,
        IntersectingTimeFrame,
        StartedInTimeFrame,
        CompletedInTimeFrame,
        TrimToTimeFrame
    }
}
