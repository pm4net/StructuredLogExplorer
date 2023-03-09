<script lang="ts">
    import { norm, normPos } from "../../../helpers/svg-helpers";
    import type { ValueTupleOfNodeAndNodeAndEdge } from "../../../shared/pm4net-client";

    export let edge : ValueTupleOfNodeAndNodeAndEdge;

    function calculatePath(edge : ValueTupleOfNodeAndNodeAndEdge) {
        let start = norm(edge.item1.coordinate);
        let end = norm(edge.item2.coordinate);
        let path = "M " + (start.x + normPos(0.45)) + "," + (start.y + normPos(0.5));
        edge.item3.waypoints.forEach(c => {
            let cPos = norm(c);
            path += " L " + cPos.x + "," + cPos.y;
        });
        path += " L " + (end.x + normPos(0.45)) + "," + end.y;
        return path;
    } 
</script>

<path class="edge" d={calculatePath(edge)}></path>

<style lang="scss">
    .edge {
        stroke: #000000;
        stroke-width: 3;
        fill: none;
    }
</style>