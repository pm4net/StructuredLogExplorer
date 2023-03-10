<script lang="ts">
    import * as d3 from "d3";
    import ZoomSvg from "@svelte-parts/zoom/svg"
    import { normPos } from "../../helpers/svg-helpers";
    import { Coordinate, DirectedGraphOfNodeAndEdge } from "../../shared/pm4net-client";
    import Node from "./svg/node.svelte";
    import Edge from "./svg/edge.svelte";

    export let dfg : DirectedGraphOfNodeAndEdge = new DirectedGraphOfNodeAndEdge({ nodes: [], edges: [] });

    let allCoordinates = 
        dfg.nodes
        .flatMap(n => n.coordinate)
        .concat(dfg.edges.flatMap(e => e.item3.waypoints))
        .filter((c): c is Coordinate => !!c); // https://www.benmvp.com/blog/filtering-undefined-elements-from-array-typescript/

    let viewBox = {
        minX: normPos(d3.min(allCoordinates, c => c.x) ?? 0),
        minY: normPos(d3.min(allCoordinates, c => c.y) ?? 0),
        maxX: normPos(d3.max(allCoordinates, c => c.x) ?? 0),
        maxY: normPos(d3.max(allCoordinates, c => c.y) ?? 0)
    };
</script>

<div class="graph">
    <ZoomSvg viewBox="{viewBox.minX} {viewBox.minY} {viewBox.maxX} {viewBox.maxY}">
        <defs>
            <marker
                id="arrowhead"
                viewBox="0 0 10 10"
                refX="1"
                refY="5"
                markerUnits="strokeWidth"
                markerWidth="3"
                markerHeight="3"
                orient="auto">
                <path d="M 0 0 L 10 5 L 0 10 z" fill="#000" />
            </marker>
        </defs>

        {#each dfg.nodes as node}
            <Node node={node}></Node>
        {/each}
        {#each dfg.edges as edge}
            <Edge edge={edge}></Edge>
        {/each}
    </ZoomSvg>
</div>

<style lang="scss">
    .graph {
        width: 100%;
        height: calc(100vh - 48px);
    }
</style>