<script lang="ts">
    import { onMount } from "svelte";
    import panzoom from "panzoom";

    export let svg : string;

    onMount(() => {
        let elem = document.getElementsByTagName("g")[0];
        if (elem) {
            panzoom(elem, {
                maxZoom: 1,
                initialZoom: 1
            });
        }
    });
</script>

{@html svg}