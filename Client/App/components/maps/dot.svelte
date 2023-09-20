<script lang="ts">
    import { Loading } from "carbon-components-svelte";
    import { Graphviz } from "@hpcc-js/wasm";
    import { onMount } from "svelte";
    import panzoom from "panzoom";

    export let dot : string;

    let graphviz = Graphviz.load();
    let svg = graphviz.then(g => g.dot(dot));

    onMount(() => {
        svg.then(_ => {
            let elem = document.getElementsByTagName("g")[0];
            if (elem) {
                panzoom(elem, {
                    minZoom: 0.05,
                    maxZoom: 1,
                    initialZoom: 1
                });
            }
        });
    });
</script>

{#await svg}
    <Loading withOverlay={false} description="Loading..." />
{:then svg}
    {@html svg}
{/await}