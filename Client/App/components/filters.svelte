<script lang="ts">
    import { Accordion, AccordionItem, Button, Checkbox, NumberInput } from "carbon-components-svelte";
    import { activeProject, mapSettings } from "../shared/stores";

    export let availableObjectTypes = <string[]>[];
    let objectTypes = $mapSettings[$activeProject ?? ""]?.objectTypes ?? [];
    let minEvents = $mapSettings[$activeProject ?? ""].dfg.minEvents;
    let minOccurrences = $mapSettings[$activeProject ?? ""].dfg.minOccurrences;
    let minSuccessions = $mapSettings[$activeProject ?? ""].dfg.minSuccessions;

    $: {
        // Update the local store settings whenever any of the referenced values change
        let settings = $mapSettings;
        settings[$activeProject ?? ""].objectTypes = objectTypes;
        settings[$activeProject ?? ""].dfg.minEvents = minEvents;
        settings[$activeProject ?? ""].dfg.minOccurrences = minOccurrences;
        settings[$activeProject ?? ""].dfg.minSuccessions = minSuccessions;
        mapSettings.set(settings);
    }
</script>

<Accordion>
    <AccordionItem open>
        <svelte:fragment slot="title"><strong>Object types</strong></svelte:fragment>
        {#each availableObjectTypes as objType}
            <Checkbox bind:group={objectTypes} labelText={objType} value={objType} />
        {/each}
    </AccordionItem>
    <AccordionItem>
        <svelte:fragment slot="title"><strong>Frequency</strong></svelte:fragment>
        <NumberInput label="Min. events in trace" min={0} bind:value={minEvents} />
        <NumberInput label="Min. occurrences" min={0} bind:value={minOccurrences} />
        <NumberInput label="Min. successions" min={0} bind:value={minSuccessions} />
    </AccordionItem>
</Accordion>