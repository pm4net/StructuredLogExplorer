<script lang="ts">
    import * as d3 from "d3";
    import { normPos } from "../../helpers/svg-helpers";
    import { Coordinate, DirectedGraphOfNodeAndEdge } from "../../shared/pm4net-client";
    import Node from "./svg/node.svelte";

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

<svg viewBox="{viewBox.minX} {viewBox.minY} {viewBox.maxX} {viewBox.maxY}">
    {#each dfg.nodes as node}
        <Node node={node}></Node>
    {/each}
</svg>

<style lang="scss">
</style>