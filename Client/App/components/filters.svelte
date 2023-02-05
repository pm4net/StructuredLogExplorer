<script lang="ts">
    import { Accordion, AccordionItem, Checkbox } from "carbon-components-svelte";
    import { activeProject, mapSettings } from "../shared/stores";

    export let availableObjectTypes = <string[]>[];
    let objectTypes = $mapSettings[$activeProject ?? ""]?.objectTypes ?? [];
    $: {
        // Update the local store settings whenever the selection changes
        let settings = $mapSettings;
        settings[$activeProject ?? ""].objectTypes = objectTypes;
        mapSettings.set(settings);
    }
</script>

<Accordion>
    <AccordionItem title="Object types" open>
        {#each availableObjectTypes as objType}
            <Checkbox bind:group={objectTypes} labelText={objType} value={objType} />
        {/each}
    </AccordionItem>
</Accordion>