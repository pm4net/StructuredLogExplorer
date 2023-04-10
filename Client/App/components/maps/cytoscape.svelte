<script lang="ts">
    import { Edge, End, Event, GraphLayout, LogLevel, Start } from "../../shared/pm4net-client";
    import { getColor } from "../../helpers/color-helpers";
    import { onMount } from "svelte";
    import cytoscape from "cytoscape";
    import Color from "color";
    import { Search } from "carbon-components-svelte";
    import nodeHtmlLabel from "cytoscape-node-html-label";
    import { placeAroundMatches } from "../../helpers/string-helpers";

    // Input to components
    export let layout : GraphLayout = new GraphLayout({ nodes: [], edges: [] });

    // Create cytoscape instance and register extensions
    let cy : cytoscape.Core;
    nodeHtmlLabel(cytoscape);

    // State values
    let searchVal : string;

    function getEdgeId(edge: Edge) {
        return edge.sourceId + "-" + edge.targetId;
    }

    function createNodesFromLayout(graph: GraphLayout) {
        return graph.nodes!.map(node => {
            let elem : cytoscape.NodeDefinition = {
                data: { 
                    id: node.id,
                    text: node.text,
                    info: node.nodeInfo,
                    type: node.nodeType
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

    function logLevelToColor(level: LogLevel) {
        switch (level) {
            case LogLevel.Verbose:
                return "#FFFFFF";
            case LogLevel.Debug:
                return "#E0E0E0";
            case LogLevel.Information:
                return "#C0C0C0";
            case LogLevel.Warning:
                return "#FF9933";
            case LogLevel.Error:
                return "#FF3333";
            case LogLevel.Fatal:
                return "#990000";
            default:
                return "#FFFFFF";
        }
    }

    // https://stackoverflow.com/a/31687097/2102106
    function scaleBetween(unscaledNum: number, minAllowed: number, maxAllowed: number, min: number, max: number) {
      return (maxAllowed - minAllowed) * (unscaledNum - min) / (max - min) + minAllowed;
    }

    function zoomToAndHighlightMatchingNodes(search: string) {
        let matchingNodes = cy.nodes().filter(function(el) {
            return el.data('text').join(' ').toLowerCase().includes(search.toLowerCase());
        });
        cy.fit(matchingNodes, 50);
        // TODO: highlight nodes somehow
    }

    onMount(() => {
        let nodes = createNodesFromLayout(layout);
        let edges = createEdgesFromLayout(layout);
        
        let edgeWeights = edges.map(e => e.e.totalWeight);
        let minEdgeWeight = Math.min(...edgeWeights);
        let maxEdgeWeight = Math.max(...edgeWeights);

        // Initialize the Cytoscape container
        cy = cytoscape({
            container: document.getElementById("dfg"),
            style: [
                {
                    selector: 'node',
                    style: {
                        'text-wrap': 'wrap',
                        'shape': 'round-rectangle',
                        'font-family': 'sans-serif',
                        'font-size': '1em',
                        'border-width': '1px',
                        'border-color': 'black'
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
                        'target-arrow-shape': 'triangle',
                    }
                }
            ],
            elements: nodes.map(n => n.elem).concat(edges.map(e => e.elem)),
            layout: {
                name: "preset",
                fit: true,
                padding: 75,
                animate: true
            },
            autoungrabify: true
        });

        let startNodeStyles = {
            'shape': 'round-rectangle',
            'border-width': '3px',
            'border-style': 'double'
        };

        let endNodeStyles = {
            'shape': 'round-rectangle',
            'border-width': '3px',
            'border-style': 'double'
        };

        // Add styling to nodes
        layout.nodes?.forEach(n => {
            let elem = cy.$id(n.id!);
            elem.style({
                //'label': n.text?.join('\n'),
                'width': n.size!.width!,
                'height': n.size!.height!
            });
            
            if (n.nodeType instanceof Event) {
                if (n.nodeInfo?.logLevel) {
                    let bgColor = logLevelToColor(n.nodeInfo.logLevel);
                    elem.style({
                        'background-color': bgColor
                    });
                } else {
                    elem.style({
                        'background-color': '#C0C0C0',
                        'color': '#000000',
                    });
                }
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
                    'width': scaleBetween(e.totalWeight, 3, 10, minEdgeWeight, maxEdgeWeight),
                });
            }

            if (e.typeInfos.length > 0) {
                let color = e.typeInfos[0].type ? getColor(e.typeInfos[0].type) : "#ccc";
                elem.style({
                    'line-color': color,
                    'target-arrow-color': color,
                });
            }
        });

        // Add HTML labels to nodes
        cy.nodeHtmlLabel([{
            query: 'node',
            halign: 'center', // title vertical position. Can be 'left',''center, 'right'
            valign: 'center', // title vertical position. Can be 'top',''center, 'bottom'
            halignBox: 'center', // title vertical position. Can be 'left',''center, 'right'
            valignBox: 'center', // title relative box vertical position. Can be 'top',''center, 'bottom'
            cssClass: 'cy-title', // any classes will be as attribute of <div> container for every title
            tpl: function(data: any) {

                // Text color calculations
                let bgColor;
                if (data.type instanceof Event) {
                    bgColor = logLevelToColor(data.info?.logLevel);
                } else {
                    bgColor = getColor(data.type.objectType);
                }
                let txtColor = Color(bgColor).isDark() ? '#FFFFFF' : "#000000";

                // Bold text calculations
                let text = data.text.join('<br>');
                text = placeAroundMatches(text, '{', '}', '<strong>', '</strong>');
                
                return `<span style="color: ${txtColor}">${text}</span>`
            }
        }]);
    });
</script>

<Search placeholder="Search nodes..." bind:value={searchVal} on:change={(_) => zoomToAndHighlightMatchingNodes(searchVal)}></Search>
<div id="dfg"></div>

<style lang="scss">
    #dfg {
        width: 100%;
        height: calc(100vh - 48px);
    }

    :global(.bx--search ) {
        position: absolute;
        z-index: 1;
        top: 1rem;
        left: 1rem;
        width: calc(100% - 2rem);
    }

    :global(.cy-title) {
        text-align: center;
    }
</style>