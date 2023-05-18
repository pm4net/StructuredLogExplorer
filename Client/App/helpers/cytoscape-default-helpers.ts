import cytoscape from "cytoscape";
import type { GraphLayout } from "../shared/pm4net-client";
import { getEdgeId } from "./cytoscape-helpers";

function createNodesFromGraph(graph: GraphLayout) {
    return graph.nodes!.map(node => {
        let elem : cytoscape.NodeDefinition = {
            data: {
                id: node.id,
                text: node.text,
                info: node.nodeInfo,
                type: node.nodeType,
                disabled: false
            }
        };
        return { elem: elem, n: node };
    });
}

function createEdgesFromGraph(graph: GraphLayout) {
    return graph.edges!.map(edge => {
        let elem : cytoscape.EdgeDefinition = {
            data: {
                id: getEdgeId(edge),
                source: edge.sourceId,
                target: edge.targetId,
                typeInfos: edge.typeInfos,
                totalWeight: edge.totalWeight
            }
        };
        return { elem: elem, e: edge };
    });
}

export function initializeBfsCytoscape(graph: GraphLayout, container: any) {
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