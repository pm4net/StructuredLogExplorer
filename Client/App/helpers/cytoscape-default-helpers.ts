import cytoscape, { type NodeDataDefinition } from "cytoscape";
import { EventNode, type DirectedGraphOfNodeOfNodeInfoAndEdgeOfEdgeInfo, StartNode, EndNode } from "../shared/pm4net-client";

function createNodesFromGraph(graph: DirectedGraphOfNodeOfNodeInfoAndEdgeOfEdgeInfo) {
    return graph.nodes!.map(node => {
        let data : NodeDataDefinition = {};
        if (node instanceof EventNode) {
            data = {
                id: node.name,
                text: node.name,
                info: node.nodeInfo,
                disabled: false
            };
        } else if (node instanceof StartNode) {
            data = {
                id: "start_" + node.type,
                text: node.type
            };
        } else if (node instanceof EndNode) {
            data = {
                id: "end_" + node.type,
                text: node.type
            };
        }

        let elem : cytoscape.NodeDefinition = {
            data: data
        };
        return { elem: elem, n: node };
    });
}

function createEdgesFromGraph(graph: DirectedGraphOfNodeOfNodeInfoAndEdgeOfEdgeInfo) {
    return graph.edges!.map(edge => {
        let elem : cytoscape.EdgeDefinition = {
            data: {
                id: "", // TODO
                source: "",
                target: "",
                edgeInfo: edge.item3
            }
        };
        return { elem: elem, e: edge };
    });
}

export function initializeBfsCytoscape(graph: DirectedGraphOfNodeOfNodeInfoAndEdgeOfEdgeInfo, container: any) {
    let nodes = createNodesFromGraph(graph);
    let edges = createEdgesFromGraph(graph);

    let cy = cytoscape({
        container: container,
        style: [],
        elements: nodes.map(n => n.elem).concat(edges.map(e => e.elem)),
        layout: {
            name: "breadthfirst",
            directed: true,
            fit: true,
            padding: 75,
            animate: true
        },
        autoungrabify: true
    });
    
    return cy;
}