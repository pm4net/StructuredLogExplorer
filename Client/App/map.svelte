<script lang="ts">
    import Layout from "./shared/layout.svelte";
    import { onMount } from "svelte";
    import { activeProject } from "./shared/stores";
    import { mapClient } from "./shared/pm4net-client-config";

    async function getOcDfg(minEvents: number, minOccurrences: number, minSuccessions: number, includedTypes: string[]) {
        try {
            return await mapClient.discoverObjectCentricDirectlyFollowsGraph($activeProject, minEvents, minOccurrences, minSuccessions, includedTypes);
        } catch (e: unknown) {
            // TODO: Show modal with error message
            console.error(e);
        }
    }

    onMount(async () => {
        let res = await getOcDfg(0, 0, 0, ["CorrelationId"]);
        console.log("res", res);
    });
</script>

<Layout>
    
</Layout>
