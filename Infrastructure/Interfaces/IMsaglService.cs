using pm4net.Types;

namespace Infrastructure.Interfaces
{
    [Obsolete("Use pm4net implementation instead")]
    public interface IMsaglService
    {
        string ComputeSvgGraph(
            string projectName,
            DirectedGraph<Node<NodeInfo>, Edge<EdgeInfo>> model,
            bool mergeEdges,
            bool groupByNamespace);
    }
}
