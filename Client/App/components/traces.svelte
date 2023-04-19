<script lang="ts">
    import { Accordion, AccordionItem, ComboBox, InlineLoading, ListItem, UnorderedList } from "carbon-components-svelte";
    import { mapClient } from "../shared/pm4net-client-config";
    import { activeProject } from "../shared/stores";
    import { OcelString, OcelValue } from "../shared/pm4net-client";

    export let objectTypes : string[];

    let selectedType : string;

    async function getTracesForObjectType(objType: string) {
        try {
            return await mapClient.getTracesForObjectType($activeProject, objType);
        } catch (e: unknown) {
            console.error(e); // TODO
        }
    }

    function getStringValue(ocelVal: OcelValue) {
        if (ocelVal instanceof OcelString) {
            return ocelVal.value;
        } else {
            return undefined;
        }
    }
</script>

<Accordion>
    <AccordionItem open>
        <svelte:fragment slot="title"><strong>Traces</strong></svelte:fragment>
        <ComboBox 
            placeholder="Select an object type"
            items={objectTypes.map(o => {return { id: o, text: o }})}
            selectedId="0"
            bind:value={selectedType}>
        </ComboBox>

        {#key selectedType}
            {#if selectedType}
                {#await getTracesForObjectType(selectedType)}
                    <InlineLoading description="Loading traces..."></InlineLoading>
                {:then traces}
                    {#if traces}
                        {#each traces as t, i}
                            <h4>Trace {i}</h4>
                            <UnorderedList>
                                {#each t as event}
                                    <ListItem>{getStringValue(event.item2.vMap["pm4net_RenderedMessage"])}</ListItem>
                                {/each}
                            </UnorderedList>
                        {/each}
                    {/if}
                {/await}
            {:else}
                <p>Please select an object type</p>
            {/if}
        {/key}
    </AccordionItem>
</Accordion>

<style lang="scss">

</style>