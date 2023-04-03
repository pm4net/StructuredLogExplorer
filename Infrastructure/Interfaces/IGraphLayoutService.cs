using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Models;
using OCEL.CSharp;
using pm4net.Types;
using static Infrastructure.Services.GraphLayoutService;

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
			float edgeSep);

		void SaveNodeCalculations(string projectName, IEnumerable<NodeCalculation> nodes);
	}
}
