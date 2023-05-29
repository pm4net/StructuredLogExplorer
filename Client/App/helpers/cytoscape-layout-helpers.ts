import cytoscape from "cytoscape";
import { End, Event, GraphLayout, Start } from "../shared/pm4net-client";
import { getColor } from "./color-helpers";
import { getEdgeId, logLevelToColor } from "./cytoscape-helpers";
import { distanceBetweenPoints, distanceToLine, pointOnLine, scaleBetween } from "./math-helpers";
import { DisplayMethod } from "../shared/stores";

function createNodesFromLayout(graph: GraphLayout) {
    return graph.nodes.map(node => {
        let elem : cytoscape.NodeDefinition = {
            data: {
                id: node.id,
                text: node.text,
                info: node.nodeInfo,
                type: node.nodeType,
                size: node.size,
                disabled: false,
                slightlyHidden: false
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
    return graph.edges.map(edge => {
        let elem : cytoscape.EdgeDefinition = {
            data: {
                id: getEdgeId(edge),
                source: edge.sourceId,
                target: edge.targetId,
                downwards: edge.downwards,
                typeInfos: edge.typeInfos,
                totalWeight: edge.totalWeight
            }
        };
        return { elem: elem, e: edge };
    });
}

export function initializeCytoscape(layout: GraphLayout, customLayout: boolean, displayMethod: DisplayMethod | undefined, container: any) {
    let nodes = createNodesFromLayout(layout);
    let edges = createEdgesFromLayout(layout);
    
    let edgeWeights = edges.map(e => e.e.totalWeight);
    let minEdgeWeight = Math.min(...edgeWeights);
    let maxEdgeWeight = Math.max(...edgeWeights);

    let cyLayout : cytoscape.LayoutOptions;
    if (customLayout) {
        cyLayout = {
            name: "preset",
            fit: true,
            padding: 75,
            animate: true
        };
    } else {
        if (displayMethod === DisplayMethod.CytoscapeBfs) {
            cyLayout = {
                name: "breadthfirst",
                directed: true,
                nodeDimensionsIncludeLabels: true,
                spacingFactor: 3,
                fit: true,
                padding: 75,
                animate: true
            }
        } else /*if (displayMethod === DisplayMethod.CytoscapeCose)*/ {
            cyLayout = {
                name: "cose",
                nodeDimensionsIncludeLabels: true,
                spacingFactor: 3,
                componentSpacing: 40,
                fit: true,
                padding: 75,
                animate: true
            }
        }
    }

    // Initialize the Cytoscape container
    let cy = cytoscape({
        container: container,
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
            },
            {
                selector: '.verbose-node-highlighted',
                style: {
                    'border-style': 'double',
                    'border-width': '3px',
                    'background-color': '#bfbfbf', // Darkened normal verbose color by 25% (https://mdigi.tools/darken-color/#ffffff)
                    'transition-property': 'background-color, border-color, border-style, border-width',
                    'transition-duration': 1000
                }
            },
            {
                selector: '.debug-node-highlighted',
                style: {
                    'border-style': 'double',
                    'border-width': '3px',
                    'background-color': '#a8a8a8', // Darkened normal warning color by 25% (https://mdigi.tools/darken-color/#e0e0e0)
                    'transition-property': 'background-color, border-color, border-style, border-width',
                    'transition-duration': 1000
                }
            },
            {
                selector: '.info-node-highlighted',
                style: {
                    'border-style': 'double',
                    'border-width': '3px',
                    'background-color': '#909090', // Darkened normal warning color by 25% (https://mdigi.tools/darken-color/#c0c0c0)
                    'transition-property': 'background-color, border-color, border-style, border-width',
                    'transition-duration': 1000
                }
            },
            {
                selector: '.warning-node-highlighted',
                style: {
                    'border-style': 'double',
                    'border-width': '3px',
                    'background-color': '#e57300', // Darkened normal warning color by 25% (https://mdigi.tools/darken-color/#ff9933)
                    'transition-property': 'background-color, border-color, border-style, border-width',
                    'transition-duration': 1000
                }
            },
            {
                selector: '.error-node-highlighted',
                style: {
                    'border-style': 'double',
                    'border-width': '3px',
                    'background-color': '#e50000', // Darkened normal warning color by 25% (https://mdigi.tools/darken-color/#ff3333)
                    'transition-property': 'background-color, border-color, border-style, border-width',
                    'transition-duration': 1000
                }
            },
            {
                selector: '.fatal-node-highlighted',
                style: {
                    'border-style': 'double',
                    'border-width': '3px',
                    'background-color': '#730000', // Darkened normal warning color by 25% (https://mdigi.tools/darken-color/#990000)
                    'transition-property': 'background-color, border-color, border-style, border-width',
                    'transition-duration': 1000
                }
            },
            {
                selector: '.edge-highlighted',
                style: {
                    'width': '3px',
                    'transition-property': 'width',
                    'transition-duration': 1000
                }
            }
        ],
        elements: nodes.map(n => n.elem).concat(edges.map(e => e.elem)),
        layout: cyLayout,
        autoungrabify: true
    });

    let startNodeStyles = {
        'shape': 'round-rectangle',
        'border-width': '3px',
        'border-style': 'double'
    };

    let endNodeStyles = {
        'shape': 'cut-rectangle',
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
    if (customLayout) {
        layout.edges.forEach(e => {
            let elem = cy.$id(getEdgeId(e));
            if (e.sourceId === e.targetId) {
                elem.style({
                    'curve-style': 'bezier'
                });
            } else {
                elem.style({
                    'width': scaleBetween(e.totalWeight, 3, 10, minEdgeWeight, maxEdgeWeight),
                });
    
                if (e.waypoints!.coordinates.length > 0) {
                    let startPos = cy.$id(e.sourceId).position();
                    let endPos = cy.$id(e.targetId).position();
                    let waypoints = e.waypoints!.coordinates.sort(p => p.y);
                    if (e.downwards) {
                        waypoints.reverse();
                    }
    
                    elem.style({
                        'curve-style': 'segments',
                        'segment-distances': e.waypoints!.coordinates.map(w => distanceToLine(startPos, endPos, w)),
                        'segment-weights': e.waypoints!.coordinates.map(w => {
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
        });
    }

    // Add coloring to edges
    layout.edges.forEach(e => {
        let elem = cy.$id(getEdgeId(e));
        if (e.typeInfos.length > 1) {
            let start = cy.$id(e.sourceId).position();
            let end = cy.$id(e.targetId).position();
            var dist = distanceBetweenPoints(start.x, start.y, end.x, end.y); // TODO: Should count distances from waypoint to waypoint for more accurate results.
            let noOfRepetitions = Math.floor(dist / 100); // How many times should we repeat the colors?

            let colors = e.typeInfos.map(t => t.type ? getColor(t.type) : "#ccc");

            let stopColors : string[] = [];
            let stopPositions : string[] = [];
            for (let i = 0; i < noOfRepetitions; i++) {
                let startOffset = i / noOfRepetitions * 100;
                for (let j = 0; j < colors.length; j++) {
                    // Append color segment
                    stopColors = stopColors.concat([colors[j], colors[j]]);

                    // Calculate position pair for segment
                    let start = j / colors.length * (startOffset > 0 ? startOffset : 100);
                    let end = (j + 1) / colors.length * (startOffset > 0 ? startOffset : 100);
                    stopPositions = stopPositions.concat([`${start}%`, `${end}%`]);
                }
            }

            elem.style({
                'target-arrow-color': stopColors.at(-1),
                'line-fill': 'linear-gradient',
                'line-gradient-stop-colors': stopColors,
                'line-gradient-stop-positions': stopPositions
            });
        } else {
            let color = e.typeInfos[0].type ? getColor(e.typeInfos[0].type) : "#ccc";
            elem.style({
                'line-color': color,
                'target-arrow-color': color,
            });
        }
    })

    return cy;
}