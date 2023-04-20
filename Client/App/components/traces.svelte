<script lang="ts">
    import { Accordion, AccordionItem, ComboBox, InlineLoading, ListItem, UnorderedList } from "carbon-components-svelte";
    import { mapClient } from "../shared/pm4net-client-config";
    import { activeProject } from "../shared/stores";
    import type { OcelValue } from "../shared/pm4net-client";

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
</script>

<Accordion>
    <AccordionItem open>
        <svelte:fragment slot="title"><strong>Traces</strong></svelte:fragment>
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
                {#each traces as t, i}
                    <h4>Trace {i}</h4>
                    <UnorderedList>
                        {#each t.item2 as event}
                            <ListItem>{getStringValue(event.item2.vMap["pm4net_RenderedMessage"])}</ListItem>
                        {/each}
                    </UnorderedList>
                {/each}
            {/if}
        {/await}
    </AccordionItem>
</Accordion>

<style lang="scss">

</style>