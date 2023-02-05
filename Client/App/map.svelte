<script lang="ts">
    import Layout from "./shared/layout.svelte";
    import { activeProject, type ProjectMapSettings, mapSettings } from "./shared/stores";
    import { mapClient } from "./shared/pm4net-client-config";
    import { EndNode, EventNode, StartNode } from "./shared/pm4net-client";
    import { getErrorMessage } from "./shared/helpers";
    import { Button, Column, Grid, Loading, Row, ToastNotification } from "carbon-components-svelte";
    import { Renew } from "carbon-icons-svelte";
    import Filters from "./components/filters.svelte";

    // The state of the error notification that is shown when an API error occurs
    let errorNotification = {
        show: false,
        message: ""
    }

    let logInfoPromise = mapClient.getLogInfo($activeProject);
    let ocDfgPromise = getOcDfg($mapSettings);

    // Load the the OC-DFG from the API, using the settings from local storage.
    async function getOcDfg(settings: ProjectMapSettings) {
        try {
            if ($activeProject) {
                if (!settings[$activeProject]) {
                    settings[$activeProject] = {
                        objectTypes: (await logInfoPromise).objectTypes,
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
    {#await logInfoPromise}
        <Loading description="Loading..." />
    {:then logInfo} 
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

        <Grid fullWidth noGutter narrow>
            <Row>
                <!-- https://carbondesignsystem.com/guidelines/2x-grid/overview/#breakpoints -->
                <Column lg={4}>
                    <Filters  availableObjectTypes={logInfo.objectTypes} />
                    <Button icon={Renew} on:click={() => {
                            ocDfgPromise = getOcDfg($mapSettings);
                            errorNotification.show = false;
                            errorNotification.message = "";
                        }}>
                        Update
                    </Button>
                </Column>
                <Column lg={12}>
                    {#await ocDfgPromise}
                        <Loading description="Loading..." />
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
                </Column>
            </Row>
        </Grid>
    {/await}
</Layout>
