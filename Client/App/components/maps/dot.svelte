<script lang="ts">
    import { Loading } from "carbon-components-svelte";
    import { Graphviz } from "@hpcc-js/wasm";
    import { onMount } from "svelte";
    import panzoom from "panzoom";
    import type { OcelObject, ValueTupleOfOcelObjectAndIEnumerableOfValueTupleOfStringAndOcelEvent, ValueTupleOfStringAndOcelEvent } from "../../shared/pm4net-client";
    import * as d3 from "d3";

    export let dot : string;

    let graphviz = Graphviz.load();
    let svg = graphviz.then(g => g.dot(dot));

    let d3elem : d3.Selection<d3.BaseType, unknown, HTMLElement, any>;
    let nodes : d3.Selection<d3.BaseType, unknown, d3.BaseType, unknown>;
    let edges : d3.Selection<d3.BaseType, unknown, d3.BaseType, unknown>;

    onMount(() => {
        svg.then(_ => {
            let svgElem = document.getElementsByTagName("g")[0];
            if (svgElem) {
                // Initialize panzoom library
                panzoom(svgElem, {
                    maxZoom: 1,
                    initialZoom: 1
                });
            }

            // Initialize d3
            d3elem = d3.select("#graph0");
            nodes = d3elem.selectAll('.node');
            edges = d3elem.selectAll('.edge');
        });
    });

    // Highlight the nodes and edges that are present in a list of traces
    export function highlightTraces(traces: ValueTupleOfOcelObjectAndIEnumerableOfValueTupleOfStringAndOcelEvent[]) {
        console.log("highlighting traces", traces);
    }

    // Highlight the nodes and edges for a specific trace, replacing the text inside of the nodes with the real rendered text
    export function highlightSpecificTrace(trace: { item1: OcelObject, item2: ValueTupleOfStringAndOcelEvent[], text: string } | null) {
        console.log("highlighting specific trace", trace);

        // Find nodes in trace
        let matchingNodes : d3.Selection<d3.BaseType, unknown, d3.BaseType, unknown>;
        trace?.item2.forEach(e => {
            console.log("iteration", e, nodes);

            let newNodes = nodes.filter(n => {
                console.log("WHY ARE YOU UNDEFINED???", n);
                // @ts-ignore
                return n.innerHTML.includes(e.item2.activity);
            });

            console.log("new nodes", newNodes);
            matchingNodes = matchingNodes.merge(newNodes);
        });

        // @ts-ignore
        console.log("found matching nodes", matchingNodes);

    }
</script>

{#await svg}
    <Loading withOverlay={false} description="Loading..." />
{:then svg}
    {@html svg}
{/await}