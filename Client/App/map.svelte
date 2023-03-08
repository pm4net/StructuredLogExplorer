<script lang="ts">
    import Layout from "./shared/layout.svelte";
    import { activeProject, mapSettings, DisplayType, DisplayMethod, EdgeType } from "./shared/stores";
    import { mapClient } from "./shared/pm4net-client-config";
    import { getErrorMessage } from "./shared/helpers";
    import { Column, Grid, InlineNotification, Loading, Row, ToastNotification } from "carbon-components-svelte";
    import Filters from "./components/filters.svelte";
    import Dfg from "./components/dfg.svelte";
    import Dot from "./components/dot.svelte";

    // The state of the error notification that is shown when an API error occurs
    let errorNotification = {
        show: false,
        message: ""
    }

    // Retrieve some basic information about the event log
    let logInfoPromise = mapClient.getLogInfo($activeProject);

    // Initialize the map settings for the currently active project if it isn't already.
    async function setMapSettingsToDefaultIfNotExists() {
        if ($activeProject) {
            if (!$mapSettings[$activeProject]) {
                $mapSettings[$activeProject] = {
                    displayType: DisplayType.OcDfg,
                    edgeType: EdgeType.Frequency,
                    displayMethod: DisplayMethod.Dot,
                    groupByNamespace: true,
                    objectTypes: [], //(await logInfoPromise).objectTypes,
                    dfg: {
                        minEvents: 0,
                        minOccurrences: 0,
                        minSuccessions: 0
                    }
                };
            }
        }
    }

    // Load the OC-DFG from the API, using the settings from local storage.
    async function getOcDfg() {
        try {
            return await mapClient.discoverOcDfgAndApplyStableGraphLayout(
                $activeProject, 
                $mapSettings[$activeProject ?? ""]?.groupByNamespace,
                $mapSettings[$activeProject ?? ""]?.dfg.minEvents, 
                $mapSettings[$activeProject ?? ""]?.dfg.minOccurrences, 
                $mapSettings[$activeProject ?? ""]?.dfg.minSuccessions, 
                $mapSettings[$activeProject ?? ""]?.objectTypes);
        } catch (e: unknown) {
            errorNotification.show = true;
            errorNotification.message = getErrorMessage(e);
        }
    }

    // Load the DOT definition of the OC-DFG from the API, using the settings from local storage.
    async function getOcDfgDot() {
        try {
            return await mapClient.discoverOcDfgAndGenerateDot(
                $activeProject,
                $mapSettings[$activeProject ?? ""]?.groupByNamespace,
                $mapSettings[$activeProject ?? ""]?.dfg.minEvents, 
                $mapSettings[$activeProject ?? ""]?.dfg.minOccurrences, 
                $mapSettings[$activeProject ?? ""]?.dfg.minSuccessions, 
                $mapSettings[$activeProject ?? ""]?.objectTypes);
        } catch (e: unknown) {
            errorNotification.show = true;
            errorNotification.message = getErrorMessage(e);
        }
    }
</script>

<Layout>
    {#if $activeProject}
        {#await logInfoPromise}
            <Loading description="Loading..." />
        {:then logInfo} 
            {#await setMapSettingsToDefaultIfNotExists() then _}
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
                        <Column sm={4} md={3} lg={4} xlg={3} max={3}>
                            <Filters availableObjectTypes={logInfo.objectTypes} />
                        </Column>
                        <Column sm={4} md={5} lg={12} xlg={13} max={13}>
                            {#key $mapSettings[$activeProject ?? ""]}
                                {#if $mapSettings[$activeProject ?? ""]?.displayType == DisplayType.OcDfg}
                                    {#if $mapSettings[$activeProject ?? ""]?.displayMethod == DisplayMethod.Dot}
                                        {#await getOcDfgDot()}
                                            <Loading description="Loading..." />
                                        {:then dot}
                                            <Dot dot={dot ?? ""}></Dot>
                                        {/await}
                                    {:else if $mapSettings[$activeProject ?? ""]?.displayMethod == DisplayMethod.Cytoscape}
                                        {#await getOcDfg()}
                                            <Loading description="Loading..." />
                                        {:then ocDfg}
                                            <Dfg dfg={ocDfg}></Dfg>
                                        {/await}
                                    {/if}
                                {:else if $mapSettings[$activeProject ?? ""]?.displayType == DisplayType.OcPn}
                                    <InlineNotification lowContrast hideCloseButton title="Currently not supported" />
                                {/if}
                            {/key}
                        </Column>
                    </Row>
                </Grid>
            {/await}
        {/await}
    {:else}
        <InlineNotification
            title="Please activate a project before visiting this page."
            subtitle="Your process map will then be shown here."
            lowContrast
            hideCloseButton />
    {/if}
</Layout>
