<script lang="ts">
    import { CodeSnippet, ExpandableTile } from "carbon-components-svelte";
    import { OcelMap, type OcelObject, type OcelValue, type ValueTupleOfStringAndOcelEvent } from "../shared/pm4net-client";
    import { DateTime } from "luxon";
    import humanizeDuration from "humanize-duration";
    import { getStringValue } from "../helpers/ocel-helpers";
    import { logLevelFromString, logLevelToColor } from "../helpers/cytoscape-helpers";
    import Color from "color";

    export let trace : { item1: OcelObject, item2: ValueTupleOfStringAndOcelEvent[], text: string } | undefined;
    export let highlightedIndex : number | null;

    $: {
        if (highlightedIndex) {
            // TODO: How to find nth tile and scroll to it?
            //var elem = document.querySelector(`bx--tile--expandable:nth-of-type(${highlightedIndex})`) as HTMLElement;
            //console.log("elem", elem);
            //elem?.scrollIntoView({ behavior: "smooth" });
        }
    }

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

    function getLogLevelColor(level: OcelValue | undefined) {
        if (level) {
            let str = getStringValue(level);
            let logLevel = logLevelFromString(str);
            let logLevelColor = logLevelToColor(logLevel);
            let lightened = Color(logLevelColor).lighten(0.1);
            return lightened;
        }
        return undefined;
    }

    function getExceptionValues(exception: OcelValue) {
        if (exception instanceof OcelMap) {
            return exception.values;
        }

        return {};
    }
</script>

{#if trace !== undefined}
    <div class="add-margin">
        {#each trace.item2 as event, idx_e}
            <ExpandableTile
                style={
                    `background:${getLogLevelColor(event.item2.vMap["pm4net_Level"])};` + 
                    (idx_e === highlightedIndex ? `outline: 5px solid greenyellow; outline-offset: -2px;` : "")
                }
            >
                <div slot="above" class="wrap-overflow">
                    <p><strong>{getStringValue(event.item2.vMap["pm4net_RenderedMessage"])}</strong></p>
                </div>
                <div slot="below" class="wrap-overflow">
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
                    {:else if event.item2.vMap["SourceContext"] !== undefined}
                        <br />
                        <strong>Namespace: </strong>{getStringValue(event.item2.vMap["SourceContext"])}
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
                            <br/><br/>
                            <strong>Attributes:</strong>
                            {#each attrs as attr}
                                <br />
                                <strong>{attr[0]}: </strong> {getStringValue(attr[1])}
                            {/each}
                        {/if}
                    {/await}

                    {#if event.item2.vMap["pm4net_Exception"] !== undefined}
                        <br/><br/>
                        <strong>Exception:</strong>
                        <br/>
                        {#await getExceptionValues(event.item2.vMap["pm4net_Exception"]) then values}
                            {#if values}
                                {#if "Message" in values}
                                    <strong>Message:</strong> {getStringValue(values["Message"])}
                                    <br/>
                                {/if}
                                {#if "Type" in values}
                                    <strong>Type:</strong> {getStringValue(values["Type"])}
                                    <br/>
                                {/if}
                                {#if "Source" in values}
                                    <strong>Source:</strong> {getStringValue(values["Source"])}
                                    <br/>
                                {/if}
                                {#if "TargetSite" in values}
                                    <strong>TargetSite:</strong> {getStringValue(values["TargetSite"])}
                                    <br/>
                                {/if}
                                {#if "HResult" in values}
                                    <strong>HResult:</strong> {getStringValue(values["HResult"])}
                                    <br/>
                                {/if}
                                {#if "StackTrace" in values}
                                    <strong>Stack trace:</strong> 
                                    <br/>
                                    <CodeSnippet type="multi" code={getStringValue(values["StackTrace"])} />
                                {/if}
                            {/if}
                        {/await}
                    {/if}
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

    .wrap-overflow {
        overflow-wrap: break-word;
    }
</style>