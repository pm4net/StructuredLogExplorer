namespace StructuredLogExplorer.Models.ControllerOptions
{
    public class GraphLayoutOptions
    {
        public bool MergeEdgesOfSameType { get; set; } = true;

        public bool FixUnforeseenEdges { get; set; } = false;

        public float NodeSeparation { get; set; } = 1f;

        public float RankSeparation { get; set; } = 2f;

        public float EdgeSeparation { get; set; } = 0.5f;
    }
}
