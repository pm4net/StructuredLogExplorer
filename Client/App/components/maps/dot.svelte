<script lang="ts">
    import { Loading } from "carbon-components-svelte";
    import { Graphviz } from "@hpcc-js/wasm";
    import { onMount } from "svelte";
    import panzoom from "panzoom";
    import type { ValueTupleOfOcelObjectAndIEnumerableOfValueTupleOfStringAndOcelEvent } from "../../shared/pm4net-client";
    import * as d3 from "d3";

    export let dot : string;

    let graphviz = Graphviz.load();
    let svg = graphviz.then(g => g.dot(dot));

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
            let d3elem = d3.select("#graph0");
            console.log("found svg", d3elem);

            let nodes = d3elem.selectAll('.node');
            console.log("found nodes", nodes);

            let childNodes = nodes.filter(n => {
                if (n instanceof HTMLElement) {
                    console.log("is html element");
                    return true;   
                }
                console.log("is not html element", typeof(n));
                return false;
            });
            console.log("child", childNodes);
        });
    });

    // Highlight the nodes and edges that are present in a list of traces
    export function highlightTraces(traces: ValueTupleOfOcelObjectAndIEnumerableOfValueTupleOfStringAndOcelEvent[]) {
        
    }
</script>

{#await svg}
    <Loading withOverlay={false} description="Loading..." />
{:then svg}
    {@html svg}
{/await}