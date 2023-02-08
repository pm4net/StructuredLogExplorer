<script lang="ts">
    import { DirectedGraphOfNodeAndEdge } from "../shared/pm4net-client";
    import { onMount } from "svelte";
    import * as d3 from "d3";

    export const dfg : DirectedGraphOfNodeAndEdge = new DirectedGraphOfNodeAndEdge({ nodes: [], edges: [] });

    onMount(() => {
        d3.select(".dfg").style("background-color", "green");
    });
</script>

<div class="dfg"></div>

<style lang="scss">
    .dfg {
        width: 100%;
        height: calc(100vh - 48px);
    }
</style>