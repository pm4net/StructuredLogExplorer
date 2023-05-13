<script lang="ts">
    import Layout from "./shared/layout.svelte";
    import { activeProject, mapSettings, DisplayType, DisplayMethod, EdgeType } from "./shared/stores";
    import { mapClient } from "./shared/pm4net-client-config";
    import { getErrorMessage, getTextSize } from "./shared/helpers";
    import { Column, Grid, InlineNotification, Loading, Row, ToastNotification } from "carbon-components-svelte";
    import Filters from "./components/filters.svelte";
    import Dot from "./components/maps/dot.svelte";
    import { GraphLayoutOptions, LogNode, NodeCalculation, OcDfgLayoutOptions, OcDfgOptions, Size, KeepCases } from "./shared/pm4net-client";
    import wrapAnsi from "wrap-ansi";
    import Traces from "./components/traces.svelte";
    import CytoscapeBfs from "./components/maps/cytoscape/cytoscape-bfs.svelte";
    import CytoscapeCustom from "./components/maps/cytoscape/cytoscape-custom.svelte";

    let cyComponent : CytoscapeCustom;

    // The state of the error notification that is shown when an API error occurs
    let errorNotification = {
        show: false,
        message: ""
    }

    // Retrieve some basic information about the event log
    let logInfoPromise = mapClient.getLogInfo($activeProject ?? "");

    // Initialize the map settings for the currently active project if it isn't already.
    async function setMapSettingsToDefaultIfNotExists() {
        if ($activeProject) {
            if (!$mapSettings[$activeProject]) {
                $mapSettings[$activeProject] = {
                    displayType: DisplayType.OcDfg,
                    edgeType: EdgeType.Frequency,
                    displayMethod: DisplayMethod.Cytoscape,
                    groupByNamespace: true,
                    objectTypes: (await logInfoPromise).objectTypes,
                    fixUnforeseenEdges: false,
                    dfg: {
                        minEvents: 0,
                        minOccurrences: 0,
                        minSuccessions: 0,
                        dateFrom: undefined,
                        dateTo: undefined,
                        keepCases: KeepCases.ContainedInTimeFrame
                    }
                };
            }
        }
    }

    async function getNodesToPreCompute() {
        try {
            return await mapClient.getAllNodesInLog($activeProject ?? "");
        } catch (e: unknown) {
            errorNotification.show = true;
            errorNotification.message = getErrorMessage(e);
        }
    }

    async function preComputeNodeProperties(nodes: LogNode[] | undefined) {
        if (nodes) {
            let computedSizes = nodes.map(n => {
                var wrapped = wrapAnsi(n.displayName, 20, { hard: false });
                var textSize = getTextSize(wrapped);
                return new NodeCalculation({
                    nodeId: n.id,
                    textWrap: wrapped.split("\n"),
                    size: new Size({ width: textSize.width * 2, height: textSize.height * 2 + 3 }),
                    nodeType: n.nodeType
                });
            });
            await mapClient.saveNodeCalculations($activeProject ?? "", computedSizes);
        }
    }

    // Load the OC-DFG from the API, using the settings from local storage.
    async function getGraphLayout() {
        try {
            return await mapClient.computeLayout($activeProject ?? "", new OcDfgLayoutOptions({
                ocDfgOptions: new OcDfgOptions({
                        minimumEvents: $mapSettings[$activeProject ?? ""]?.dfg.minEvents,
                        minimumOccurrence: $mapSettings[$activeProject ?? ""]?.dfg.minOccurrences,
                        minimumSuccessions: $mapSettings[$activeProject ?? ""]?.dfg.minSuccessions,
                        includedTypes: $mapSettings[$activeProject ?? ""]?.objectTypes,
                        dateFrom: $mapSettings[$activeProject ?? ""]?.dfg.dateFrom,
                        dateTo: $mapSettings[$activeProject ?? ""]?.dfg.dateTo,
                        keepCases: $mapSettings[$activeProject ?? ""]?.dfg.keepCases
                    }),
                    layoutOptions: new GraphLayoutOptions({
                        mergeEdgesOfSameType: true,
                        fixUnforeseenEdges: $mapSettings[$activeProject ?? ""]?.fixUnforeseenEdges,
                        nodeSeparation: 50,
                        rankSeparation: 50,
                        edgeSeparation: 50,
                        tension: 0.5
                    })
            }));
        } catch (e: unknown) {
            errorNotification.show = true;
            errorNotification.message = getErrorMessage(e);
        }
    }

    // Load the DOT definition of the OC-DFG from the API, using the settings from local storage.
    async function getOcDfgDot() {
        try {
            return await mapClient.discoverOcDfgAndGenerateDot(
                $activeProject ?? "",
                $mapSettings[$activeProject ?? ""]?.groupByNamespace,
                new OcDfgOptions({ 
                    minimumEvents: $mapSettings[$activeProject ?? ""]?.dfg.minEvents,
                    minimumOccurrence: $mapSettings[$activeProject ?? ""]?.dfg.minOccurrences,
                    minimumSuccessions: $mapSettings[$activeProject ?? ""]?.dfg.minSuccessions,
                    includedTypes: $mapSettings[$activeProject ?? ""]?.objectTypes,
                    keepCases: $mapSettings[$activeProject ?? ""]?.dfg.keepCases
                })
            );
        } catch (e: unknown) {
            errorNotification.show = true;
            errorNotification.message = getErrorMessage(e);
        }
    }

    // Load the OC-DFG from the API, using the settings from local storage
    async function getOcDfg() {
        try {
            return await mapClient.discoverOcDfg($activeProject ?? "", new OcDfgOptions({ 
                    minimumEvents: $mapSettings[$activeProject ?? ""]?.dfg.minEvents,
                    minimumOccurrence: $mapSettings[$activeProject ?? ""]?.dfg.minOccurrences,
                    minimumSuccessions: $mapSettings[$activeProject ?? ""]?.dfg.minSuccessions,
                    includedTypes: $mapSettings[$activeProject ?? ""]?.objectTypes,
                    keepCases: $mapSettings[$activeProject ?? ""]?.dfg.keepCases
                })
            );
        } catch (e: unknown) {
            errorNotification.show = true;
            errorNotification.message = getErrorMessage(e);
        }
    }

    function forwardHighlightTracesEvent(event: any) {
        cyComponent.highlightTraces(event.detail);
    }
</script>

<Layout>
    {#if $activeProject}
        {#await logInfoPromise}
            <Loading withOverlay={false} description="Loading..." />
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
                        <Column class="maxScreenHeight" sm={4} md={2} lg={4} xlg={3} max={3}>
                            <Filters availableObjectTypes={logInfo.objectTypes} minDate={logInfo.firstEventTimestamp} maxDate={logInfo.lastEventTimestamp} />
                        </Column>
                        <Column class="maxScreenHeight relativePos" sm={4} md={4} lg={8} xlg={10} max={10}>
                            <!-- Refresh whenever any of the map settings change -->
                            {#key $mapSettings[$activeProject ?? ""]}
                                {#if $mapSettings[$activeProject ?? ""]?.displayType == DisplayType.OcDfg}
                                    {#if $mapSettings[$activeProject ?? ""]?.displayMethod == DisplayMethod.Dot}
                                        {#await getOcDfgDot()}
                                            <Loading withOverlay={false} description="Loading..." />
                                        {:then dot}
                                            <Dot dot={dot ?? ""}></Dot>
                                        {/await}
                                    {:else if $mapSettings[$activeProject ?? ""]?.displayMethod == DisplayMethod.Cytoscape}
                                        {#await getNodesToPreCompute() then nodes}
                                            {#await preComputeNodeProperties(nodes) then _}
                                                {#await getGraphLayout()}
                                                    <Loading withOverlay={false} description="Loading..." />
                                                {:then layout}
                                                    <CytoscapeCustom bind:this={cyComponent} layout={layout}></CytoscapeCustom>
                                                {/await}
                                            {/await}
                                        {/await}
                                    {:else if $mapSettings[$activeProject ?? ""]?.displayMethod == DisplayMethod.CytoscapeBfs}
                                        {#await getOcDfg()}
                                            <Loading withOverlay={false} description="Loading..." />
                                        {:then ocdfg}
                                            <CytoscapeBfs ocdfg={ocdfg}></CytoscapeBfs>
                                        {/await}
                                    {/if}
                                {:else if $mapSettings[$activeProject ?? ""]?.displayType == DisplayType.OcPn}
                                    <InlineNotification lowContrast hideCloseButton title="Currently not supported" />
                                {/if}
                            {/key}
                        </Column>
                        <Column class="maxScreenHeight" sm={4} md={2} lg={4} xlg={3} max={3}>
                            <Traces objectTypes={logInfo.objectTypes} on:highlightTraces={forwardHighlightTracesEvent} />
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

<style lang="scss">
    :global(.maxScreenHeight) {
        height: calc(100vh - 48px);
        overflow: auto;
        padding-right: 0 !important;
    }

    :global(.relativePos) {
        position: relative;
    }
</style>
