<script lang="ts">
    import { ExpandableTile } from "carbon-components-svelte";
    import type { OcelObject, ValueTupleOfStringAndOcelEvent } from "../shared/pm4net-client";
    import { DateTime } from "luxon";
    import humanizeDuration from "humanize-duration";
    import { getStringValue } from "../helpers/ocel-helpers";

    export let trace : { item1: OcelObject, item2: ValueTupleOfStringAndOcelEvent[], text: string } | undefined;

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

    async function getFilteredAttributes(event: ValueTupleOfStringAndOcelEvent) {
        return Promise.resolve(Object.entries(event.item2.vMap).filter(e => !e[0].includes("pm4net_") && e[0] !== "SourceContext"));
    }
</script>

{#if trace !== undefined}
    <div class="add-margin">
        {#each trace.item2 as event, idx_e}
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

                    {#await getFilteredAttributes(event) then attrs}
                        {#if attrs.length > 0}
                            <br /><br />
                            <strong>Attributes:</strong>
                            {#each attrs as attr}
                                <br />
                                <strong>{attr[0]}: </strong> {getStringValue(attr[1])}
                            {/each}
                        {/if}
                    {/await}
                </div>
            </ExpandableTile>
            {#if idx_e < trace.item2.length - 1}
                <div class="connector">
                    <i class="arrow down" />
                    <p>{getDurationBetweenTwoEvents(trace.item2.at(idx_e), trace.item2.at(idx_e + 1))}</p>
                    <i class="arrow down" />
                </div>
            {/if}
        {/each}
    </div>
{/if}

<style lang="scss">
    .connector {
        display: flex;
        flex-direction: row;
        justify-content: center;
        align-items: center;
    }

    .add-margin {
        margin: 1rem;
    }
</style>