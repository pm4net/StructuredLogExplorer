namespace StructuredLogExplorer.Models.ControllerOptions
{
    public class OcDfgOptions
    {
        public int MinimumEvents { get; set; } = 0;

        public int MinimumOccurrence { get; set; } = 0;

        public int MinimumSuccessions { get; set; } = 0;

        public IEnumerable<string> IncludedTypes { get; set; } = Enumerable.Empty<string>();
    }
}
