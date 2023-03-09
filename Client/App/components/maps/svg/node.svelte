<script lang="ts">
    import { norm } from "../../../helpers/svg-helpers";
    import type { Node } from "../../../shared/pm4net-client";

    export let node : Node;

    let coords = norm(node.coordinate);
</script>

<g>
    <rect x={coords.x} y={coords.y} width="10" height="5" rx="1" ry="1"></rect>
</g>