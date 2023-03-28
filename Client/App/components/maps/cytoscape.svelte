<script lang="ts">
    import { Edge, End, Event, GraphLayout, Start } from "../../shared/pm4net-client";
    import { onMount } from "svelte";
    import cytoscape from "cytoscape";

    export let layout : GraphLayout = new GraphLayout({ nodes: [], edges: [] });
    let cy : cytoscape.Core;

    const scaleFactor = 25;

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
                    x: node.position!.x * scaleFactor, 
                    y: node.position!.y * scaleFactor
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
                        'shape': 'rectangle',
                        'font-family': 'monospace'
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
                        'target-arrow-shape': 'triangle'
                    }
                }
            ],
            elements: nodes.map(n => n.elem).concat(edges.map(e => e.elem)),
            layout: {
                name: "preset",
                fit: true,
                animate: false
            },
            autoungrabify: true
        });

        let eventNodeStyles = {
            'background-color': '#000000',
            'color': '#ffffff'
        };

        let startNodeStyles = {
            'background-color': '#00ff00',
            'color': '#fff'
        };

        let endNodeStyles = {
            'background-color': '#ff0000',
            'color': '#fff'
        };

        // Add styling to nodes
        layout.nodes?.forEach(n => {
            let elem = cy.$id(n.id!);
            elem.style({
                'label': n.text?.join('\n'),
                'width': n.size!.width! * scaleFactor / 2,
                'height': n.size!.height! * scaleFactor
            });
            
            if (n.nodeType instanceof Event) {
                elem.style(eventNodeStyles);
            } else if (n.nodeType instanceof Start) {
                elem.style(startNodeStyles);
            } else if (n.nodeType instanceof End) {
                elem.style(endNodeStyles);
            }
        });

        // Add styling to edges
        layout.edges?.forEach(e => {
            let elem = cy.$id(getEdgeId(e));
            elem.style({
                'curve-style': 'bezier', // TODO: make bezier curves with waypoints
                'width': Math.max(e.totalWeight / maxEdgeWeight * 15, 5),
            });
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