<script lang="ts">
    import Layout from "./shared/layout.svelte";
    
    import { activeProject, type MapSettings, type ProjectMapSettings } from "./shared/stores";
    import { mapSettings } from "./shared/stores";
    
    import { mapClient } from "./shared/pm4net-client-config";
    import { EndNode, EventNode, StartNode } from "./shared/pm4net-client";
    import type { Writable } from "svelte/store";
    import { getErrorMessage } from "./shared/helpers";
    import { ToastNotification } from "carbon-components-svelte";

    // The state of the error notification that is shown when an API error occurs
    let errorNotification = {
        show: false,
        message: ""
    }

    // A promise that refreshes every time the map settings change
    let ocDfgPromise = getOcDfg($mapSettings);

    // Load the the OC-DFG from the API, using the settings from local storage.
    async function getOcDfg(settings: ProjectMapSettings) {
        try {
            if ($activeProject) {
                if (!settings[$activeProject]) {
                    settings[$activeProject] = {
                        objectTypes: [],
                        dfg: {
                            minEvents: 0,
                            minOccurrences: 0,
                            minSuccessions: 0
                        }
                    };
                    mapSettings.set(settings);
                }

                return await mapClient.discoverObjectCentricDirectlyFollowsGraph(
                    $activeProject, 
                    settings[$activeProject].dfg.minEvents, 
                    settings[$activeProject].dfg.minOccurrences, 
                    settings[$activeProject].dfg.minSuccessions, 
                    settings[$activeProject].objectTypes); 
            }
        } catch (e: unknown) {
            errorNotification.show = true;
            errorNotification.message = getErrorMessage(e);
        }
    }
</script>

<Layout>
    {#await ocDfgPromise}
        <p>Loading...</p>
    {:then ocDfg}

        {#if errorNotification.show}
            <ToastNotification
                title="An error occurred"
                subtitle={errorNotification.message}
                kind="error"
                fullWidth
                lowContrast
                on:close={() => errorNotification.message = ""}
            />
        {/if}

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
