<script lang="ts">
    import { onMount } from "svelte";
    import { Graphviz } from "@hpcc-js/wasm";
    
    export let dot : string;

    let svg : string;
    onMount(async () => {
        const graphviz = await Graphviz.load();
        svg = graphviz.dot(dot);
    });
</script>

{@html svg}

<div id="#graph" style="text-align: center;"></div>