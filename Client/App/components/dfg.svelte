<script lang="ts">
    import { DirectedGraphOfNodeAndEdge, EndNode, EventNode, Node, StartNode } from "../shared/pm4net-client";
    import { onMount } from "svelte";
    import cytoscape from "cytoscape";

    export let dfg : DirectedGraphOfNodeAndEdge = new DirectedGraphOfNodeAndEdge({ nodes: [], edges: [] });

    function getNodeName(node: Node) {
        if (node instanceof StartNode) {
            return "Start: " + node.type;
        } else if (node instanceof EndNode) {
            return "End: " + node.type;
        } else if (node instanceof EventNode) {
            return node.name;
        } else {
            return "";
        }
    }

    function createElementsFromDirectedGraph(graph: DirectedGraphOfNodeAndEdge) {
        let nodes = graph.nodes.map(node => {
            let elem : cytoscape.NodeDefinition = {
                data: { 
                    id: getNodeName(node)
                },
                position: { 
                    x: node.coordinate ? node.coordinate.x * 100 : 0, 
                    y: node.coordinate ? node.coordinate.y * 100 : 0
                } 
            };
            return elem;
        });

        let edges = graph.edges.map(edge => {
            let elem : cytoscape.EdgeDefinition = {
                data: {
                    id: getNodeName(edge.item1) + "-" + getNodeName(edge.item2),
                    source: getNodeName(edge.item1),
                    target: getNodeName(edge.item2)
                }
            };
            
            return elem;
        });

        return nodes.concat(edges);
    }

    onMount(() => {
        // Initialize the Cytoscape container
        var cy = cytoscape({
            container: document.getElementById("dfg"),
            elements: createElementsFromDirectedGraph(dfg),
            style: [
                {
                    selector: 'node',
                    style: {
                        'background-color': '#666',
                        'label': 'data(id)'
                    }
                },

                {
                selector: 'edge',
                    style: {
                        'width': 3,
                        'line-color': '#ccc',
                        'target-arrow-color': '#ccc',
                        'target-arrow-shape': 'triangle',
                        'curve-style': 'unbundled-bezier'
                    }
                }
            ],
            layout: {
                name: "preset",
                fit: true,
                animate: false
            }
        });
    });
</script>

<div id="dfg"></div>

<style lang="scss">
    #dfg {
        width: 100%;
        height: calc(100vh - 48px);
    }
</style>