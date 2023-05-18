<script lang="ts">
    import { Loading } from "carbon-components-svelte";
    import { Graphviz } from "@hpcc-js/wasm";

    export let dot : string;

    let graphviz = Graphviz.load();

    // TODO: Doesn't work because the SVG isn't rendered when this is called, and there seems to be no good way to await its rendering that actually works :/
    /*let svgViewer = svgPanZoom("#graph", {
        panEnabled: true,
        zoomEnabled: true
    });*/
</script>

{#await graphviz}
    <Loading withOverlay={false} description="Loading..." />
{:then graphviz}
    <div id="graph">
        {@html graphviz.dot(dot)}
    </div>
{/await}