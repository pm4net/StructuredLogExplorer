using Infrastructure.Interfaces;
using Microsoft.Msagl.Core.Geometry;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.Layout.Layered;
using Microsoft.Msagl.Miscellaneous;
using Microsoft.Msagl.Prototype.Ranking;
using pm4net.Types;
using System.Text;

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
                var (wrapped, lines) = WrapText(nodeName, 20);
                var n = graph.AddNode(nodeName);
                n.LabelText = wrapped;
                n.Label.Width = (wrapped.Split(Environment.NewLine).MaxBy(l => l.Length)?.Length ?? 60) * 10;
                n.Label.Height = lines * 20;
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
                n.GeometryNode.BoundaryCurve = CurveFactory.CreateRectangleWithRoundedCorners(n.Label.Width, n.Label.Height, 3, 2, new Point(n.Label.Width, n.Label.Height));
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

        private static (string, int) WrapText(string text, int maxCharsPerLine)
        {
            var words = text.Split(' ');
            var newSentence = new StringBuilder();
            var noOfLines = 0;

            var line = "";
            foreach (var word in words)
            {
                if ((line + word).Length > maxCharsPerLine)
                {
                    newSentence.AppendLine(line);
                    noOfLines++;
                    line = "";
                }

                line += $"{word} ";
            }

            if (line.Length > 0)
            {
                newSentence.AppendLine(line);
                noOfLines++;
            }

            return (newSentence.ToString(), noOfLines);
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
