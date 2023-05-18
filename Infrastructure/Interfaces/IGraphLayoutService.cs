using Infrastructure.Models;
using OCEL.CSharp;
using pm4net.Types;
using NodeInfo = pm4net.Types.NodeInfo;

namespace Infrastructure.Interfaces
{
	public interface IGraphLayoutService
	{
		GraphLayout ComputeGraphLayout(
			string projectName,
			OcelLog log,
			GraphTypes.DirectedGraph<InputTypes.Node<NodeInfo>, InputTypes.Edge<EdgeInfo>> model,
			bool mergeEdges,
			bool fixUnforeseenEdges,
			float nodeSep,
			float rankSep,
			float edgeSep,
			float tension);

		void SaveNodeCalculations(string projectName, IEnumerable<NodeCalculation> nodes);

        IEnumerable<NodeCalculation> GetNodeCalculations(string projectName);
    }
}
