<script lang="ts">
    import { Edge, End, Event, GraphLayout, Start } from "../../shared/pm4net-client";
    import { onMount } from "svelte";
    import cytoscape from "cytoscape";
    import { getColor } from "../../helpers/color-helpers";

    export let layout : GraphLayout = new GraphLayout({ nodes: [], edges: [] });
    let cy : cytoscape.Core;

    function getEdgeId(edge: Edge) {
        return edge.sourceId + "-" + edge.targetId;
    }

    function createNodesFromLayout(graph: GraphLayout) {
        return graph.nodes!.map(node => {
            let elem : cytoscape.NodeDefinition = {
                data: { 
                    id: node.id
                },
                position: { 
                    x: node.position!.x, 
                    y: node.position!.y
                },
                
            };
            return { elem: elem, n: node };
        });
    }

    function createEdgesFromLayout(graph: GraphLayout) {
        return graph.edges!.map(edge => {
            let elem : cytoscape.EdgeDefinition = {
                data: {
                    id: getEdgeId(edge),
                    source: edge.sourceId!,
                    target: edge.targetId!
                }
            };
            return { elem: elem, e: edge };
        });
    }

    onMount(() => {
        let nodes = createNodesFromLayout(layout);
        let edges = createEdgesFromLayout(layout);
        let maxEdgeWeight = Math.max(...edges.map(e => e.e.totalWeight));

        // Initialize the Cytoscape container
        cy = cytoscape({
            container: document.getElementById("dfg"),
            style: [
                {
                    selector: 'node',
                    style: {
                        'text-valign': 'center',
                        'text-halign': 'center',
                        'text-wrap': 'wrap',
                        'shape': 'round-rectangle',
                        'font-family': 'sans-serif',
                        'font-size': '1em'
                    }
                },
                {
                    selector: ':parent',
                    style: {
                        'text-valign': 'top',
                        'text-halign': 'center',
                    }
                },
                {
                selector: 'edge',
                    style: {
                        'width': 3,
                        'line-color': '#ccc',
                        'target-arrow-color': '#ccc',
                        'target-arrow-shape': 'triangle',
                    }
                }
            ],
            elements: nodes.map(n => n.elem).concat(edges.map(e => e.elem)),
            layout: {
                name: "preset",
                fit: true,
                padding: 100,
                animate: true
            },
            autoungrabify: true
        });

        let eventNodeStyles = {
            'background-color': '#D3D3D3',
            'color': '#000000',
        };

        let startNodeStyles = {
            'color': '#fff',
            'shape': 'round-rectangle',
        };

        let endNodeStyles = {
            'color': '#fff',
            'shape': 'round-rectangle',
        };

        // Add styling to nodes
        layout.nodes?.forEach(n => {
            let elem = cy.$id(n.id!);
            elem.style({
                'label': n.text?.join('\n'),
                'width': n.size!.width!,
                'height': n.size!.height!
            });
            
            if (n.nodeType instanceof Event) {
                elem.style(eventNodeStyles);
            } else if (n.nodeType instanceof Start) {
                elem.style(startNodeStyles);
                elem.style({
                    'background-color': getColor(n.nodeType.objectType)
                });
            } else if (n.nodeType instanceof End) {
                elem.style(endNodeStyles);
                elem.style({
                    'background-color': getColor(n.nodeType.objectType)
                });
            }
        });

        // Add styling to edges
        layout.edges?.forEach(e => {
            let elem = cy.$id(getEdgeId(e));
            if (e.sourceId === e.targetId) {
                elem.style({
                    'curve-style': 'bezier'
                });
            } else {
                elem.style({
                    'curve-style': 'bezier', // TODO: make bezier curves with waypoints
                    'width': Math.max(e.totalWeight / maxEdgeWeight * 15, 5),
                });
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