<script lang="ts">
    import Layout from "./shared/layout.svelte";
    
    import { activeProject } from "./shared/stores";
    import { mapClient } from "./shared/pm4net-client-config";
    import { EndNode, EventNode, StartNode } from "./shared/pm4net-client";

    let ocDfg = getOcDfg(0, 0, 0, ["CorrelationId"]);

    async function getOcDfg(minEvents: number, minOccurrences: number, minSuccessions: number, includedTypes: string[]) {
        try {
            return await mapClient.discoverObjectCentricDirectlyFollowsGraph($activeProject, minEvents, minOccurrences, minSuccessions, includedTypes);
        } catch (e: unknown) {
            // TODO: Show modal with error message
            console.error(e);
        }
    }
</script>

<Layout>
    {#await ocDfg}
        <p>Loading...</p>
    {:then ocDfg}
        {#if ocDfg}
            {#each ocDfg.nodes as node}
                {#if node instanceof StartNode}
                    <p>{"StartNode " + node.type}</p>
                {:else if node instanceof EndNode}
                    <p>{"EndNode " + node.type}</p>
                {:else if node instanceof EventNode}
                    <p>{"EventNode " + node.name}</p>
                {/if}
            {/each}
        {/if}
    {/await}
</Layout>
