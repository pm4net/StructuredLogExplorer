import cytoscape from "cytoscape";
import type { Position } from "cytoscape";
import { Coordinate, Edge, End, Event, GraphLayout, Start } from "../shared/pm4net-client";
import { getColor } from "./color-helpers";
import { logLevelToColor } from "./cytoscape-helpers";

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
                type: node.nodeType,
                disabled: false
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
                target: edge.targetId!,
                downwards: edge.downwards,
                typeInfos: edge.typeInfos,
                totalWeight: edge.totalWeight
            }
        };
        return { elem: elem, e: edge };
    });
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

export function initializeCustomCytoscape(layout: GraphLayout, container: any) {
    let nodes = createNodesFromLayout(layout);
    let edges = createEdgesFromLayout(layout);
    
    let edgeWeights = edges.map(e => e.e.totalWeight);
    let minEdgeWeight = Math.min(...edgeWeights);
    let maxEdgeWeight = Math.max(...edgeWeights);

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
    });

    return cy;
}