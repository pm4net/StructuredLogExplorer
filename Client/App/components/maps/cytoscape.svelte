<script lang="ts">
    import { Coordinate, Edge, End, Event, GraphLayout, LogLevel, Start, ValueTupleOfOcelObjectAndIEnumerableOfValueTupleOfStringAndOcelEvent } from "../../shared/pm4net-client";
    import { getColor } from "../../helpers/color-helpers";
    import { onMount } from "svelte";
    import cytoscape, { type Position } from "cytoscape";
    import Color from "color";
    import { Button, Search } from "carbon-components-svelte";
    import nodeHtmlLabel from "cytoscape-node-html-label";
    import viewUtilities from "cytoscape-view-utilities";
    import { placeAroundMatches } from "../../helpers/string-helpers";
    import { Save } from "carbon-icons-svelte";
    import { saveAs } from "file-saver";
    import { activeProject } from "../../shared/stores";

    // Input to components
    export let layout : GraphLayout = new GraphLayout({ nodes: [], edges: [] });

    // Create cytoscape instance and register extensions
    let cy : cytoscape.Core;
    nodeHtmlLabel(cytoscape);
    viewUtilities(cytoscape);

    // State values
    let searchVal : string;
    let viewUtilitiesApi : any;

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

    // Calculate perpendicular distance of point from line defined by two points (https://en.wikipedia.org/wiki/Distance_from_a_point_to_a_line#Line_defined_by_two_points)
    function distanceToLine(p1: Position, p2: Position, point: Coordinate) {
        let part1 = (p2.x - p1.x) * (p1.y - point.y);
        let part2 = (p1.x - point.x) * (p2.y - p1.y);
        let lower = (p2.x - p1.x)**2 + (p2.y - p1.y)**2;
        let result = Math.abs(part1 - part2) / Math.sqrt(lower);
        return isLeftOfLine(p1, p2, point) ? result : -result;
    }

    // Calculate whether a given point is left of the line (https://stackoverflow.com/a/3461533/2102106)
    function isLeftOfLine(p1: Position, p2: Position, point: Coordinate) {
        return ((p2.x - p1.x) * (point.y - p1.y) - (p2.y - p1.y) * (point.x - p1.x)) > 0;
    }

    // https://stackoverflow.com/a/64122266/2102106
    function pointOnLine(p1: Position, p2: Position, q: Coordinate) {
        if (p1.x == p2.x && p1.y == p2.y) {
            p1.x -= 0.00001;
        } 

        const Unumer = ((q.x - p1.x) * (p2.x - p1.x)) + ((q.y - p1.y) * (p2.y - p1.y));
        const Udenom = Math.pow(p2.x - p1.x, 2) + Math.pow(p2.y - p1.y, 2);
        const U = Unumer / Udenom;

        const r = {
            x: p1.x + (U * (p2.x - p1.x)),
            y: p1.y + (U * (p2.y - p1.y))
        }

        const minx = Math.min(p1.x, p2.x);
        const maxx = Math.max(p1.x, p2.x);
        const miny = Math.min(p1.y, p2.y);
        const maxy = Math.max(p1.y, p2.y);

        const isValid = (r.x >= minx && r.x <= maxx) && (r.y >= miny && r.y <= maxy);

        return isValid ? r : null;
    }

    function distanceBetweenPoints(p1x: number, p1y: number, p2x: number, p2y: number) {
        return Math.sqrt((p2x - p1x)**2 + (p2y - p1y)**2);
    }

    function zoomToAndHighlightMatchingNodesAndEdges(search: string) {
        // Remove highlighting from all nodes and edges
        viewUtilitiesApi.removeHighlights(cy.nodes());
        viewUtilitiesApi.removeHighlights(cy.edges());

        // Find nodes that match the search query
        let matchingNodes = cy.nodes().filter(function(el) {
            return el.data('text').join(' ').toLowerCase().includes(search.toLowerCase());
        });

        // Find edges that connect the matching nodes and take the union with the nodes
        let connectingEdges = matchingNodes.edgesWith(matchingNodes);
        let eles = matchingNodes.union(connectingEdges);

        // Zoom the matching nodes into view
        viewUtilitiesApi.zoomToSelected(eles);

        // Highlight nodes when the search query is non-empty
        if (search !== "") {
            viewUtilitiesApi.highlight(eles); // Use first style in the list
        }
    }

    // Highlight the nodes and edges that are present in a list of traces
    export function highlightTraces(traces: ValueTupleOfOcelObjectAndIEnumerableOfValueTupleOfStringAndOcelEvent[]) {
        // TODO
    }

    async function saveGraphAsImage() {
        let pngBlob = await cy.png({
            output: 'blob-promise',
            full: true
        });
        saveAs(pngBlob, `${$activeProject}.png`);
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
                    'width': scaleBetween(e.totalWeight, 3, 10, minEdgeWeight, maxEdgeWeight),
                });

                if (e.waypoints.coordinates.length > 0) {
                    let startPos = cy.$id(e.sourceId).position();
                    let endPos = cy.$id(e.targetId).position();
                    let waypoints = e.waypoints.coordinates.sort(p => p.y);
                    if (e.downwards) {
                        waypoints.reverse();
                    }

                    elem.style({
                        'curve-style': 'segments',
                        'segment-distances': e.waypoints.coordinates.map(w => distanceToLine(startPos, endPos, w)),
                        'segment-weights': e.waypoints.coordinates.map(w => {
                            let point = pointOnLine(startPos, endPos, w);
                            if (point) {
                                let distBetweenPoints = distanceBetweenPoints(startPos.x, startPos.y, point.x, point.y);
                                let distBetweenStartEnd = distanceBetweenPoints(startPos.x, startPos.y, endPos.x, endPos.y);
                                return distBetweenPoints / distBetweenStartEnd;
                            } else {
                                return null;
                            }
                        }).flatMap(v => !!v ? [v] : []),
                        'edge-distances': 'node-position',
                    });
                } else {
                    elem.style({
                        'curve-style': 'straight'
                    });
                }
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

        // Initialize view utilities extension
        var options = {
            highlightStyles: [
                { node: { 'border-color': '#0b9bcd',  'border-width': 3 }, edge: {'line-color': '#0b9bcd', 'target-arrow-color': '#0b9bcd'} }, // Active
                { node: { 'opacity': 0.5 }, edge: { 'opacity': 0.5 } }, // Inactive
            ],
            selectStyles: {},
            zoomAnimationDuration: 1000, // default duration for zoom animation speed
        };
        viewUtilitiesApi = cy.viewUtilities(options);
    });
</script>

<Search placeholder="Search nodes..." bind:value={searchVal} on:change={(_) => zoomToAndHighlightMatchingNodesAndEdges(searchVal)}></Search>
<Button kind="secondary" iconDescription="Save image" icon={Save} tooltipPosition="left" on:click={((_) => saveGraphAsImage())}></Button>
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
        width: calc(100% - 6rem);
    }

    :global(.bx--btn.bx--btn--icon-only.bx--tooltip__trigger) {
        position: absolute;
        z-index: 1;
        top: 1rem;
        right: 1rem;
    }

    :global(.cy-title) {
        text-align: center;
    }
</style>