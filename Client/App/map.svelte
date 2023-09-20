<script lang="ts">
    import Layout from "./shared/layout.svelte";
    import { activeProject, mapSettings, DisplayType, DisplayMethod, EdgeType } from "./shared/stores";
    import { mapClient } from "./shared/pm4net-client-config";
    import { createOcDfgOptionsFromStore, getErrorMessage, getTextSize } from "./shared/helpers";
    import { Column, Grid, InlineNotification, Loading, Row, ToastNotification } from "carbon-components-svelte";
    import Filters from "./components/filters.svelte";
    import Dot from "./components/maps/dot.svelte";
    import { GraphLayoutOptions, LogNode, NodeCalculation, OcDfgLayoutOptions, Size, KeepCases, LogLevel } from "./shared/pm4net-client";
    import wrapAnsi from "wrap-ansi";
    import Traces from "./components/traces.svelte";
    import Cytoscape from "./components/maps/cytoscape.svelte";
    import Msagl from "./components/maps/msagl.svelte";

    let cyComponent : Cytoscape;
    let tracesComponent : Traces;

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
                    objectTypes: [], //(await logInfoPromise).objectTypes,
                    logLevels: [LogLevel.Unknown, LogLevel.Information, LogLevel.Warning, LogLevel.Error, LogLevel.Fatal],
                    namespaces: [], //(await logInfoPromise).namespaces,
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
                let wrapped = wrapAnsi(n.displayName, 20, { hard: false });
                let textSize = getTextSize(document.getElementsByTagName("canvas")?.[0] || document.createElement("canvas"), wrapped);
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
                ocDfgOptions: createOcDfgOptionsFromStore(),
                layoutOptions: new GraphLayoutOptions({
                    mergeEdgesOfSameType: true,
                    fixUnforeseenEdges: $mapSettings[$activeProject ?? ""]?.fixUnforeseenEdges,
                    nodeSeparation: 25,
                    rankSeparation: 10,
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
                createOcDfgOptionsFromStore());
        } catch (e: unknown) {
            errorNotification.show = true;
            errorNotification.message = getErrorMessage(e);
        }
    }

    // Load the MSAGL SVG definition of the OC-DFG from API, using the settings from local storage.
    async function getOcDfgMsagl() {
        try {
            return await mapClient.discoverOcDfgAndGenerateMsaglSvg(
                $activeProject ?? "", 
                $mapSettings[$activeProject ?? ""].groupByNamespace, 
                createOcDfgOptionsFromStore());
        } catch (e: unknown) {
            errorNotification.show = true;
            errorNotification.message = getErrorMessage(e);
        }
    }

    // Load the OC-DFG from the API, using the settings from local storage
    async function getOcDfg() {
        try {
            return await mapClient.discoverOcDfg($activeProject ?? "", createOcDfgOptionsFromStore());
        } catch (e: unknown) {
            errorNotification.show = true;
            errorNotification.message = getErrorMessage(e);
        }
    }

    function forwardHighlightTracesEvent(event: any) {
        cyComponent?.highlightTraces(event.detail);
    }

    function forwardHighlightSpecificTraceEvent(event: any) {
        cyComponent?.highlightSpecificTrace(event.detail);
    }

    function forwardAnimationStepChanged(event: any) {
        tracesComponent?.highlightAnimationStep(event.detail);
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
                            <Filters availableObjectTypes={logInfo.objectTypes} highLevelNamespaces={logInfo.namespaces} minDate={logInfo.firstEventTimestamp} maxDate={logInfo.lastEventTimestamp} />
                        </Column>
                        <Column class="maxScreenHeight relativePos noOverflow" sm={4} md={4} lg={8} xlg={10} max={10}>
                            <!-- Refresh whenever any of the map settings change -->
                            {#key $mapSettings[$activeProject ?? ""]}
                                {#if $mapSettings[$activeProject ?? ""]?.displayType == DisplayType.OcDfg}
                                    {#if $mapSettings[$activeProject ?? ""]?.displayMethod == DisplayMethod.Dot}
                                        {#await getOcDfgDot()}
                                            <Loading withOverlay={false} description="Loading..." />
                                        {:then dot}
                                            <Dot dot={dot ?? ""} />
                                        {/await}
                                    {:else if $mapSettings[$activeProject ?? ""]?.displayMethod == DisplayMethod.Msagl}
                                        {#await getOcDfgMsagl()}
                                            <Loading withOverlay={false} description="Loading..." />
                                        {:then svg}
                                            <Msagl svg={svg ?? ""}></Msagl>
                                        {/await}
                                    {:else}
                                        {#await getNodesToPreCompute() then nodes}
                                            {#await preComputeNodeProperties(nodes) then _}
                                                {#if $mapSettings[$activeProject ?? ""]?.displayMethod == DisplayMethod.Cytoscape}
                                                    {#await getGraphLayout()}
                                                        <Loading withOverlay={false} description="Loading..." />
                                                    {:then layout}
                                                        <Cytoscape 
                                                            bind:this={cyComponent} 
                                                            layout={layout} 
                                                            on:animateStepChanged={forwardAnimationStepChanged} 
                                                        />
                                                    {/await}
                                                {:else}
                                                    {#await getOcDfg()}
                                                        <Loading withOverlay={false} description="Loading..." />
                                                    {:then ocdfg}
                                                        <Cytoscape 
                                                            bind:this={cyComponent} 
                                                            ocdfg={ocdfg} 
                                                            layoutEngine={$mapSettings[$activeProject ?? ""]?.displayMethod}
                                                            on:animateStepChanged={forwardAnimationStepChanged}
                                                        />
                                                    {/await}
                                                {/if}
                                            {/await}
                                        {/await}
                                    {/if}
                                {:else if $mapSettings[$activeProject ?? ""]?.displayType == DisplayType.OcPn}
                                    <InlineNotification lowContrast hideCloseButton title="Currently not supported" />
                                {/if}
                            {/key}
                        </Column>
                        <Column class="maxScreenHeight" sm={4} md={2} lg={4} xlg={3} max={3}>
                            <!-- Refresh traces window when filters change too, so that the traces can be reloaded according to the applied filters, and re-highlighted -->
                            {#key $mapSettings[$activeProject ?? ""]}
                                <!-- When it changes, we can pass in the selected type as a prop (have to somehow know what was selected before though) -->
                                <Traces
                                    bind:this={tracesComponent}
                                    objectTypes={$mapSettings[$activeProject ?? ""].objectTypes}
                                    on:highlightTraces={forwardHighlightTracesEvent}
                                    on:highlightSpecificTrace={forwardHighlightSpecificTraceEvent}
                                />
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

<style lang="scss">
    :global(.maxScreenHeight) {
        height: calc(100vh - 48px);
        overflow: auto;
        padding-right: 0 !important;
    }

    :global(.relativePos) {
        position: relative;
    }

    :global(.noOverflow) {
        overflow: hidden;
    }
</style>
