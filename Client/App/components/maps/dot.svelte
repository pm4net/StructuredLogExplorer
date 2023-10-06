<script lang="ts">
    import { Loading } from "carbon-components-svelte";
    import { Graphviz } from "@hpcc-js/wasm";
    import { onMount } from "svelte";
    import panzoom from "panzoom";
    import type { OcelObject, ValueTupleOfOcelObjectAndIEnumerableOfValueTupleOfStringAndOcelEvent, ValueTupleOfStringAndOcelEvent } from "../../shared/pm4net-client";

    export let dot : string;

    let graphviz = Graphviz.load();
    let svgStr = graphviz.then(g => g.dot(dot));

    let svgContainer : SVGElement | null;
    let allNodes : Element[] = [];
    let allEdges : Element[] = [];

    let previouslyMatchedNodes : Element[] = [];
    let previouslyMatchedEdges : [Element, string][] = []; // Tuple of element and corresponding type color
    let previousStartNode : [Element, string] | null;
    let previousEndNode : [Element, string] | null;
    let previousStartEdge : [Element, string] | null;
    let previousEndEdge : [Element, string] | null;

    onMount(() => {
        svgStr.then(_ => {
            let svgElem = document.getElementsByTagName("g")[0];
            if (svgElem) {
                // Initialize panzoom library
                panzoom(svgElem, {
                    maxZoom: 1,
                    initialZoom: 1
                });
            }

            // Initialize interactivity
            svgContainer = document.querySelector("#graph0");
            allNodes = Array.from(svgContainer?.querySelectorAll(".node") ?? []);
            allEdges = Array.from(svgContainer?.querySelectorAll(".edge") ?? []);
        });
    });

    // Highlight the nodes and edges that are present in a list of traces
    export function highlightTraces(traces: ValueTupleOfOcelObjectAndIEnumerableOfValueTupleOfStringAndOcelEvent[]) {
        
    }

    // Highlight the nodes and edges for a specific trace, replacing the text inside of the nodes with the real rendered text
    export function highlightSpecificTrace(trace: { item1: OcelObject, item2: ValueTupleOfStringAndOcelEvent[], text: string } | null) {

        // Reset highlights on previos nodes
        resetStyles();

        // Find start and end node for object type
        let startNode = allNodes.find(n => n.querySelector("title")?.textContent?.includes(`StartNode ${trace?.item1.type}`));
        let endNode = allNodes.find(n => n.querySelector("title")?.textContent?.includes(`EndNode ${trace?.item1.type}`));

        let objTypeColor = startNode?.querySelector("ellipse")?.style.fill;

        if (startNode) {
            let ellipse = startNode.querySelector("ellipse");
            if (ellipse) {
                ellipse.style.stroke = "red";
                ellipse.style.strokeWidth = "3";
            }

            previousStartNode = [startNode, objTypeColor ?? ""];
        }

        if (endNode) {
            let polyline = endNode.querySelector("polyline");
            let text = endNode.querySelector("text");
            if (polyline && text) {
                polyline.style.stroke = "red";
                text.style.fill = "red";
            }

            previousEndNode = [endNode, objTypeColor ?? ""];
        }

        // Find start and end edges for object type
        let startEdge = allEdges.find(n => n.querySelector("title")?.textContent?.includes(`StartNode ${trace?.item1.type}->${trace?.item2[0].item2.activity}`));
        let endEdge = allEdges.find(n => n.querySelector("title")?.textContent?.includes(`${trace?.item2[trace.item2.length - 1].item2.activity}->EndNode ${trace?.item1.type}`));

        if (startEdge) {
            let path = startEdge.querySelector("path");
            let polygon = startEdge.querySelector("polygon");
            if (path && polygon) {
                path.style.stroke = "red";
                polygon.style.fill = "red";
                polygon.style.stroke = "red";
            }

            previousStartEdge = [startEdge, objTypeColor ?? ""];
        }

        if (endEdge) {
            let path = endEdge.querySelector("path");
            let polygon = endEdge.querySelector("polygon");
            if (path && polygon) {
                path.style.stroke = "red";
                polygon.style.fill = "red";
                polygon.style.stroke = "red";
            }

            previousEndEdge = [endEdge, objTypeColor ?? ""];
        }


        // Find nodes in trace
        let matchingNodes : Element[] = [];
        trace?.item2.forEach(e => {
            let newNodes = allNodes.filter(n => n.querySelector("title")?.textContent?.includes(e.item2.activity));
            matchingNodes = matchingNodes.concat(newNodes);
        });

        // Highlight matching nodes
        matchingNodes.forEach(n => {
            let polygon = n.querySelector("polygon");
            if (polygon) {
                polygon.style.stroke = "red";
                polygon.style.strokeWidth = "3";
            }
        });
        previouslyMatchedNodes = matchingNodes;

        // Find edges in trace
        let matchingEdges : [Element, string][] = [];
        for (let i = 0; i < (trace?.item2.length ?? 0) - 1; i++) {
            let curr = trace?.item2[i];
            let next = trace?.item2[i + 1];
            if (curr && next) {
                let edgeTitle = curr.item2.activity + "->" + next.item2.activity;
                let newEdges = allEdges.filter(e => e.querySelector("title")?.textContent?.includes(edgeTitle));
                let edgesWithColors : [Element, string][] = newEdges.map(e => {
                    let path = e.querySelector("path");
                    return [e, path?.style.stroke ?? ""];
                });
                matchingEdges = matchingEdges.concat(edgesWithColors);
            }
        }

        // Highlight matching edges
        matchingEdges.forEach(e => {
            let polygon = e[0].querySelector("polygon");
            let path = e[0].querySelector("path");
            if (polygon && path) {
                polygon.style.fill = "red";
                polygon.style.stroke = "red";
                path.style.stroke = "red";
            }
        });
        previouslyMatchedEdges = matchingEdges;
    }

    // Reset previously highlighted elements to their defaults
    function resetStyles() {
        if (previousStartNode) {
            let startEllipse = previousStartNode[0].querySelector("ellipse");
            if (startEllipse) {
                startEllipse.style.stroke = "black";
                startEllipse.style.strokeWidth = "1";
            }
        }
        
        if (previousEndNode) {
            let endPolyline = previousEndNode[0].querySelector("polyline");
            let endText = previousEndNode[0].querySelector("text");
            if (endPolyline && endText) {
                endPolyline.style.stroke = previousEndNode[1];
                endText.style.fill = previousEndNode[1];
            }
        }

        if (previousStartEdge) {
            let polygon = previousStartEdge[0].querySelector("polygon");
            let path = previousStartEdge[0].querySelector("path");
            if (polygon && path) { 
                polygon.style.fill = previousStartEdge[1];
                polygon.style.stroke = previousStartEdge[1];
                path.style.stroke = previousStartEdge[1];
            }
        }

        if (previousEndEdge) {
            let polygon = previousEndEdge[0].querySelector("polygon");
            let path = previousEndEdge[0].querySelector("path");
            if (polygon && path) { 
                polygon.style.fill = previousEndEdge[1];
                polygon.style.stroke = previousEndEdge[1];
                path.style.stroke = previousEndEdge[1];
            }
        }

        previouslyMatchedNodes.forEach(n => {
            let polygon = n.querySelector("polygon");
            if (polygon) {
                polygon.style.stroke = "black";
                polygon.style.strokeWidth = "1";
            }
        });

        previouslyMatchedEdges.forEach(e => {
            let polygon = e[0].querySelector("polygon");
            let path = e[0].querySelector("path");
            if (polygon && path) { 
                polygon.style.fill = e[1];
                polygon.style.stroke = e[1];
                path.style.stroke = e[1];
            }
        })
    }
</script>

{#await svgStr}
    <Loading withOverlay={false} description="Loading..." />
{:then svg}
    {@html svg}
{/await}