<script lang="ts">
    import { Button, ComboBox, InlineLoading } from "carbon-components-svelte";
    import { mapClient } from "../shared/pm4net-client-config";
    import { activeProject } from "../shared/stores";
    import { DateTime } from "luxon";
    import { createEventDispatcher } from "svelte";
    import { createOcDfgOptionsFromStore } from "../shared/helpers";
    import type { OcelObject, ValueTupleOfStringAndOcelEvent } from "../shared/pm4net-client";
    import type { ComboBoxItem } from "carbon-components-svelte/types/ComboBox/ComboBox.svelte";
    import Trace from "./trace.svelte";
    import { getStringValue } from "../helpers/ocel-helpers";
    import { Debug } from "carbon-icons-svelte";

    // Props
    export let objectTypes : string[];
    export let selectedType = ""; // $mapSettings[$activeProject ?? ""]?.selectedTypeForTraces ?? "";
    export let selectedObjectText = "";

    const dispatch = createEventDispatcher();

    let tracesPromise = getTracesForObjectType(selectedType);
    async function getTracesForObjectType(objType: string) {
        try {
            if (objType) {
                return (await mapClient.getTracesForObjectType($activeProject, objType, createOcDfgOptionsFromStore())).map(t => { return { 
                    item1: t.item1,
                    item2: t.item2,
                    text: getTraceTitle(t.item1)
                }});
            }
        } catch (e: unknown) {
            console.error(e); // TODO
        }
    }

    function getTraceTitle(obj: OcelObject) {
        let keys = Object.keys(obj.ovMap ?? {});
        let fstObj = obj.ovMap![keys[0]];
        if (fstObj) {
            return getStringValue(fstObj);
        } else {
            return "";
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

    async function loadAndHighlightTraces() {
        tracesPromise = getTracesForObjectType(selectedType);
        dispatch("highlightTraces", await tracesPromise);
    }

    async function loadAndHighlightSpecificTrace() {
        dispatch("highlightSpecificTrace", (await tracesPromise)?.find(t => t.text === selectedObjectText))
    }

    function shouldFilterItem(item: ComboBoxItem, value: string) {
        if (!value) return true;
        return item.text?.toLowerCase().includes(value.toLowerCase());
    }
</script>

<ComboBox 
    placeholder="Select an object type"
    items={objectTypes.map(o => { return { id: o, text: o } })}
    bind:value={selectedType}
    on:clear={() => {
        dispatch("highlightTraces", []);
        // Set the traces to an empty array to avoid loading element showing, but hiding previously loaded content
        tracesPromise = Promise.resolve([]);
    }}
    on:select={loadAndHighlightTraces}
    {shouldFilterItem}>
</ComboBox>

{#await tracesPromise}
    <InlineLoading description="Loading traces..."></InlineLoading>
{:then traces}
    {#if traces && traces.length > 0}
        <ComboBox
            placeholder="Select an object identifier"
            items={traces.map(t => { return { 
                id: t,
                text: t.text
            }})}
            bind:value={selectedObjectText}
            on:clear={() => dispatch("highlightSpecificTrace", undefined)}
            on:select={loadAndHighlightSpecificTrace}
            let:item>
            <div>
                <strong>{item.text}</strong>
            </div>
            <div>
                {item.id.item2.length} events ({getDateString(item.id.item2.at(0))} - {getDateString(item.id.item2.at(-1))})
            </div>
        </ComboBox>

        {#if selectedObjectText && selectedObjectText !== ""}
            <div class="add-button-margin">
                <Button
                    icon={Debug}
                    size="small"
                    on:click={() => {}}>
                    Replay trace    
                </Button>
            </div>
        {/if}

        <Trace trace={traces.find(t => t.text === selectedObjectText)}></Trace>
    {/if}
{/await}

<style lang="scss">
    .add-button-margin {
        margin-left: 1rem;
        margin-right: 1rem;
    }

    :global(.bx--list-box__menu-item, .bx--list-box__menu-item__option) {
        height: auto;
    }

    :global(.bx--list-box__wrapper) {
        padding: 1rem;
    }

    :global(.bx--list-box__wrapper:nth-child(1)) {
        padding-bottom: 0;
    }
</style>