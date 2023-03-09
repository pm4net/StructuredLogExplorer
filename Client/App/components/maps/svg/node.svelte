<script lang="ts">
    import { norm, normPos } from "../../../helpers/svg-helpers";
    import { EndNode, EventNode, StartNode, type Node } from "../../../shared/pm4net-client";

    export let node : Node;

    let mouseOver = false;

    let coords = norm(node.coordinate);
    let text : string;
    let color : string;

    $: {
        if (node instanceof EventNode) {
            text = node.name;
            color = mouseOver ? "gray" : "lightGray";
        } else if (node instanceof StartNode) {
            text = "Start " + node.type;
            color = mouseOver ? "darkGreen" : "green";
        } else if (node instanceof EndNode) {
            text = "End " + node.type;
            color = mouseOver ? "darkRed" : "red";
        }
    }
</script>

<g class="node" on:click={() => console.log("clicked", node)} on:mouseover={() => mouseOver = true} on:mouseleave={() => mouseOver = false}>
    <rect x={coords.x} y={coords.y} width={normPos(0.9)} height={normPos(0.5)} rx="5" ry="5" fill={color} stroke="black"></rect>
    <foreignObject x={coords.x + normPos(0.01)} y={coords.y + normPos(0.01)} width={normPos(0.9 - 0.01)} height={normPos(0.5 - 0.01)}>
        <p class="label">{text}</p>
    </foreignObject>
</g>

<style lang="scss">
    .node {
        cursor: pointer;
    }

    .label {
        font-size: small;
        text-align: center;
    }
</style>