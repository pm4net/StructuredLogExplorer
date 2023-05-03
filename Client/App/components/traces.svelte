<script lang="ts">
    import { Accordion, AccordionItem, ComboBox, ExpandableTile, InlineLoading } from "carbon-components-svelte";
    import { mapClient } from "../shared/pm4net-client-config";
    import { activeProject } from "../shared/stores";
    import type { OcelObject, OcelValue, ValueTupleOfStringAndOcelEvent } from "../shared/pm4net-client";
    import { DateTime } from "luxon";
    import humanizeDuration from "humanize-duration"
    import { createEventDispatcher } from "svelte";

    // Props
    export let objectTypes : string[];

    const dispatch = createEventDispatcher();

    // State variables
    let selectedType : string = "";

    // Humanizer
    const shortEnglishHumanizer = humanizeDuration.humanizer({
        language: "shortEn",
        spacer: "",
        languages: {
            shortEn: {
                y: () => "y",
                mo: () => "mo",
                w: () => "w",
                d: () => "d",
                h: () => "h",
                m: () => "m",
                s: () => "s",
                ms: () => "ms",
                },
            },
        });

    let tracesPromise = getTracesForObjectType(selectedType);
    async function getTracesForObjectType(objType: string) {
        try {
            if (objType) {
                return await mapClient.getTracesForObjectType($activeProject, objType);
            }
        } catch (e: unknown) {
            console.error(e); // TODO
        }
    }

    function getStringValue(ocelVal: OcelValue) : string {
        // @ts-ignore
        return ocelVal.value;

        /*if (ocelVal instanceof OcelString) {
            return ocelVal.value;
        } else {
            return undefined;
        }*/
    }

    function getTraceTitle(obj: OcelObject) {
        let keys = Object.keys(obj.ovMap ?? {});
        let fstObj = obj.ovMap![keys[0]];
        if (fstObj) {
            return getStringValue(fstObj);
        } else {
            return undefined;
        }
    }

    function getDateString(obj: ValueTupleOfStringAndOcelEvent | undefined) {
        if (obj) {
            let lx = DateTime.fromJSDate(obj.item2.timestamp);
            return lx.toLocaleString(DateTime.DATETIME_SHORT_WITH_SECONDS);
        } else {
            return undefined;
        }
    }

    function getDurationBetweenTwoEvents(e1: ValueTupleOfStringAndOcelEvent | undefined, e2: ValueTupleOfStringAndOcelEvent | undefined) {
        if (e1 && e2) {
            let lx1 = DateTime.fromJSDate(e1.item2.timestamp);
            let lx2 = DateTime.fromJSDate(e2.item2.timestamp);
            let duration = lx2.diff(lx1, ['milliseconds']);
            // Using external library until this is resolved: // https://github.com/moment/luxon/issues/1134
            return shortEnglishHumanizer(duration.milliseconds, { units: ['y', 'mo', 'w', 'd', 'h', 'm', 's', 'ms'] });
        } else {
            return undefined;
        }
    }
</script>

<strong>Traces</strong>
<ComboBox 
    placeholder="Select an object type"
    items={objectTypes.map(o => {return { id: o, text: o }})}
    selectedId="0"
    bind:value={selectedType}
    on:clear={(_) => dispatch("highlightTraces", [])}
    on:select={async (_) => {
        tracesPromise = getTracesForObjectType(selectedType);
        dispatch("highlightTraces", await tracesPromise);
    }}>
</ComboBox>

{#await tracesPromise}
    <InlineLoading description="Loading traces..."></InlineLoading>
{:then traces}
    {#if traces}
        <Accordion>
            {#each traces as t, idx_t}
                <AccordionItem open={false}>
                    <svelte:fragment slot="title">
                        <h5>{getTraceTitle(t.item1)}</h5>
                        <div>{t.item2.length} events ({getDateString(t.item2.at(0))} - {getDateString(t.item2.at(-1))})</div>
                    </svelte:fragment>
                    {#each t.item2 as event, idx_e}
                        <ExpandableTile>
                            <div slot="above">
                                <p><strong>{getStringValue(event.item2.vMap["pm4net_RenderedMessage"])}</strong></p>
                            </div>
                            <div slot="below">
                                <strong>Template: </strong>{event.item2.activity}
                                <br />
                                <strong>Timestamp: </strong>{DateTime.fromJSDate(event.item2.timestamp).toLocaleString(DateTime.DATETIME_FULL_WITH_SECONDS)}
                                {#if event.item2.vMap["pm4net_Level"] !== undefined}
                                    <br />
                                    <strong>Level: </strong>{getStringValue(event.item2.vMap["pm4net_Level"])}
                                {/if}
                                {#if event.item2.vMap["pm4net_Namespace"] !== undefined}
                                    <br />
                                    <strong>Namespace: </strong>{getStringValue(event.item2.vMap["pm4net_Namespace"])}
                                {/if}
                                {#if event.item2.vMap["pm4net_SourceFile"] !== undefined}
                                    <br />
                                    <strong>Source File: </strong>{getStringValue(event.item2.vMap["pm4net_SourceFile"])}
                                {/if}
                                {#if event.item2.vMap["pm4net_LineNumber"] !== undefined && event.item2.vMap["pm4net_ColumnNumber"] !== undefined}
                                    <br />
                                    <strong>Line number: </strong>{getStringValue(event.item2.vMap["pm4net_LineNumber"])}, Col. {getStringValue(event.item2.vMap["pm4net_ColumnNumber"])}
                                {/if}
                            </div>
                        </ExpandableTile>
                        {#if idx_e < t.item2.length - 1}
                            <div class="connector">
                                <i class="arrow down" />
                                <p>{getDurationBetweenTwoEvents(t.item2.at(idx_e), t.item2.at(idx_e + 1))}</p>
                                <i class="arrow down" />
                            </div>
                        {/if}
                    {/each}
                </AccordionItem>
            {/each}
        </Accordion>
    {/if}
{/await}

<style lang="scss">
    .connector {
        display: flex;
        flex-direction: row;
        justify-content: center;
        align-items: center;
    }
</style>