<script lang="ts">
    import { norm, normPos } from "../../../helpers/svg-helpers";
    import type { ValueTupleOfNodeAndNodeAndEdge } from "../../../shared/pm4net-client";

    export let edge : ValueTupleOfNodeAndNodeAndEdge;

    function calculatePath(edge : ValueTupleOfNodeAndNodeAndEdge) {
        let downwards = edge.item1.coordinate!.y < edge.item2.coordinate!.y;
        let start = norm(edge.item1.coordinate);
        let end = norm(edge.item2.coordinate);
        let path = "M " + (start.x + normPos(0.45)) + "," + (start.y + (downwards ? normPos(0.5) : 0));
        edge.item3.waypoints.forEach(c => {
            let cPos = norm(c);
            path += " L " + (cPos.x + normPos(0.45)) + "," + (cPos.y + normPos(0.25));
        });
        path += " L " + (end.x + normPos(0.45)) + "," + (downwards ? end.y : end.y + normPos(0.5));
        return path;
    } 
</script>

<g>
    <path class="edge {edge.item3.waypoints.length == 0 ? "edge--normal" : "edge--controlled"}" d={calculatePath(edge)} marker-end="url(#arrowhead)"></path>
    {#each edge.item3.waypoints as waypoint}
        <circle cx={normPos(waypoint.x) + normPos(0.45)} cy={normPos(waypoint.y) + normPos(0.25)} r={normPos(0.1)} fill="red"></circle>
    {/each}
</g>

<style lang="scss">
    .edge {
        stroke-width: 3;
        fill: none;

        &--normal {
            stroke: #000000;
        }

        &--controlled {
            stroke: #00ff00;
        }
    }
</style>