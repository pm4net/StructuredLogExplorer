using Infrastructure.Interfaces;
using Microsoft.Msagl.Core.Geometry;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.Layout.Layered;
using Microsoft.Msagl.Miscellaneous;
using Microsoft.Msagl.Prototype.Ranking;
using pm4net.Types;

namespace Infrastructure.Services
{
    public class MsaglService : IMsaglService
    {
        public string ComputeSvgGraph(string projectName, DirectedGraph<Node<NodeInfo>, Edge<EdgeInfo>> model, bool mergeEdges, bool groupByNamespace)
        {
            var graph = new Graph();

            foreach (var node in model.Nodes)
            {
                var nodeName = GetNodeName(node);
                var n = graph.AddNode(nodeName);
                n.LabelText = nodeName;
                n.Label.Width = 100;
                n.Label.Height = 50;
            }

            foreach (var edge in model.Edges)
            {
                var source = GetNodeName(edge.Item1);
                var target = GetNodeName(edge.Item2);
                var e = graph.AddEdge(source, target);
                e.LabelText = $"{source} -> {target}";
            }

            graph.CreateGeometryGraph();

            // Now the drawing graph elements point to the corresponding geometry elements, however the node boundary curves are not set. Setting the node boundaries
            foreach (var n in graph.Nodes)
            {
                n.GeometryNode.BoundaryCurve = CurveFactory.CreateRectangleWithRoundedCorners(60, 40, 3, 2, new Point(0, 0));
            }
            
            LayoutHelpers.CalculateLayout(graph.GeometryGraph, new SugiyamaLayoutSettings(), null);
            return GetSvg(graph);
        }

        private static string GetNodeName(Node<NodeInfo> node)
        {
            string? objType = null;
            EventNode<NodeInfo>? eventNode = null;

            if (node.TryEventNode(ref eventNode))
            {
                return eventNode.Name;
            }

            if (node.TryStartNode(ref objType))
            {
                return $"{nameof(Node<NodeInfo>.StartNode)} {objType}";
            }

            if (node.TryEndNode(ref objType))
            {
                return $"{nameof(Node<NodeInfo>.EndNode)} {objType}";
            }

            throw new ArgumentOutOfRangeException(nameof(node), "Node is not one of the allowed types.");
        }

        private static string GetSvg(Graph drawingGraph)
        {
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            var svgWriter = new SvgGraphWriter(writer.BaseStream, drawingGraph);
            svgWriter.Write();
            ms.Position = 0;
            var sr = new StreamReader(ms);
            var myStr = sr.ReadToEnd();
            return myStr;
        }
    }
}
