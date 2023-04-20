<script lang="ts">
    import { Accordion, AccordionItem, ComboBox, InlineLoading, ListItem, UnorderedList } from "carbon-components-svelte";
    import { mapClient } from "../shared/pm4net-client-config";
    import { activeProject } from "../shared/stores";
    import type { OcelEvent, OcelObject, OcelValue, ValueTupleOfStringAndOcelEvent } from "../shared/pm4net-client";
    import { DateTime } from "luxon";

    export let objectTypes : string[];

    let selectedType : string = "";

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

    function getStringValue(ocelVal: OcelValue) {
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
            return "";
        }
    }
</script>

<strong>Traces</strong>
<ComboBox 
    placeholder="Select an object type"
    items={objectTypes.map(o => {return { id: o, text: o }})}
    selectedId="0"
    bind:value={selectedType}
    on:select={(_) => tracesPromise = getTracesForObjectType(selectedType)}>
</ComboBox>

{#await tracesPromise}
    <InlineLoading description="Loading traces..."></InlineLoading>
{:then traces}
    {#if traces}
        <Accordion>
            {#each traces as t, i}
                <AccordionItem open={false}>
                    <svelte:fragment slot="title">
                        <h5>{getTraceTitle(t.item1)}</h5>
                        <div>{t.item2.length} events ({getDateString(t.item2.at(0))} - {getDateString(t.item2.at(-1))})</div>
                    </svelte:fragment>
                    <UnorderedList>
                        {#each t.item2 as event}
                            <ListItem>{getStringValue(event.item2.vMap["pm4net_RenderedMessage"])}</ListItem>
                        {/each}
                    </UnorderedList>
                </AccordionItem>
            {/each}
        </Accordion>
    {/if}
{/await}

<style lang="scss">

</style>