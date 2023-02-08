<script lang="ts">
    import { onMount } from "svelte";
    import * as d3 from "d3";

    export let dot : string;

    onMount(() => {
        d3.select("#graph").graphviz().renderDot(dot);
    });
</script>

<div id="#graph"></div>