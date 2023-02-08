<script lang="ts">
    import { Accordion, AccordionItem, Button, Checkbox, FormGroup, NumberInput, RadioButton, RadioButtonGroup } from "carbon-components-svelte";
    import { activeProject, DisplayMethod, DisplayType, mapSettings } from "../shared/stores";

    export let availableObjectTypes = <string[]>[];
    let displayType = $mapSettings[$activeProject ?? ""]?.displayType;
    let displayMethod = $mapSettings[$activeProject ?? ""]?.displayMethod;
    let objectTypes = $mapSettings[$activeProject ?? ""]?.objectTypes ?? [];
    let minEvents = $mapSettings[$activeProject ?? ""].dfg.minEvents;
    let minOccurrences = $mapSettings[$activeProject ?? ""].dfg.minOccurrences;
    let minSuccessions = $mapSettings[$activeProject ?? ""].dfg.minSuccessions;

    $: {
        // Update the local store settings whenever any of the referenced values change
        let settings = $mapSettings;
        settings[$activeProject ?? ""].displayType = displayType;
        settings[$activeProject ?? ""].displayMethod = displayMethod;
        settings[$activeProject ?? ""].objectTypes = objectTypes;
        settings[$activeProject ?? ""].dfg.minEvents = minEvents;
        settings[$activeProject ?? ""].dfg.minOccurrences = minOccurrences;
        settings[$activeProject ?? ""].dfg.minSuccessions = minSuccessions;
        mapSettings.set(settings);
    }
</script>

<Accordion>
    <AccordionItem open>
        <svelte:fragment slot="title"><strong>Display</strong></svelte:fragment>
        <FormGroup>
            <RadioButtonGroup orientation="vertical" legendText="Display type" bind:selected={displayType}>
                <RadioButton labelText="Object-Centric Directly Follows Graph" value={DisplayType.OcDfg} />
                <RadioButton labelText="Object-Centric Petri Net" value={DisplayType.OcPn} disabled />
            </RadioButtonGroup>
        </FormGroup>
        <FormGroup noMargin>
            <RadioButtonGroup orientation="vertical" legendText="Display method" bind:selected={displayMethod}>
                <RadioButton labelText="DOT" value={DisplayMethod.Dot} />
                <RadioButton labelText="Custom" value={DisplayMethod.Custom} disabled />
            </RadioButtonGroup>
        </FormGroup>
    </AccordionItem>
    <AccordionItem>
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