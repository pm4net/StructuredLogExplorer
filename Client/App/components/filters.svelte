<script lang="ts">
    import { Accordion, AccordionItem, Button, Checkbox, FormGroup, NumberInput, RadioButton, RadioButtonGroup, Toggle } from "carbon-components-svelte";
    import { activeProject, DisplayMethod, DisplayType, EdgeType, mapSettings } from "../shared/stores";

    export let availableObjectTypes = <string[]>[];
    let displayType = $mapSettings[$activeProject ?? ""]?.displayType;
    let edgeType = $mapSettings[$activeProject ?? ""]?.edgeType;
    let displayMethod = $mapSettings[$activeProject ?? ""]?.displayMethod;
    let groupByNamespace = $mapSettings[$activeProject ?? ""]?.groupByNamespace;
    let objectTypes = $mapSettings[$activeProject ?? ""]?.objectTypes ?? [];
    let minEvents = $mapSettings[$activeProject ?? ""].dfg.minEvents;
    let minOccurrences = $mapSettings[$activeProject ?? ""].dfg.minOccurrences;
    let minSuccessions = $mapSettings[$activeProject ?? ""].dfg.minSuccessions;

    $: {
        // Update the local store settings whenever any of the referenced values change
        let settings = $mapSettings;
        settings[$activeProject ?? ""].displayType = displayType;
        settings[$activeProject ?? ""].edgeType = edgeType;
        settings[$activeProject ?? ""].displayMethod = displayMethod;
        settings[$activeProject ?? ""].groupByNamespace = groupByNamespace;
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
        <FormGroup>
            <RadioButtonGroup orientation="vertical" legendText="Edge type" bind:selected={edgeType}>
                <RadioButton labelText="Frequency" value={EdgeType.Frequency} />
                <RadioButton labelText="Performance" value={EdgeType.Performance} disabled />
            </RadioButtonGroup>
        </FormGroup>
        <FormGroup>
            <RadioButtonGroup orientation="vertical" legendText="Display method" bind:selected={displayMethod}>
                <RadioButton labelText="DOT" value={DisplayMethod.Dot} />
                <RadioButton labelText="Cytoscape" value={DisplayMethod.Cytoscape} />
                <RadioButton labelText="D3" value={DisplayMethod.D3} />
            </RadioButtonGroup>
        </FormGroup>
        <FormGroup noMargin>
            <Toggle labelText="Group by namespace" bind:toggled={groupByNamespace}></Toggle>
        </FormGroup>
    </AccordionItem>
    <AccordionItem open>
        <svelte:fragment slot="title"><strong>Frequency</strong></svelte:fragment>
        <NumberInput label="Min. events in trace" min={0} bind:value={minEvents} />
        <NumberInput label="Min. occurrences" min={0} bind:value={minOccurrences} />
        <NumberInput label="Min. successions" min={0} bind:value={minSuccessions} />
    </AccordionItem>
    <AccordionItem open>
        <svelte:fragment slot="title"><strong>Object types</strong></svelte:fragment>
        {#each availableObjectTypes as objType}
            <Checkbox bind:group={objectTypes} labelText={objType} value={objType} />
        {/each}
    </AccordionItem>
</Accordion>

<style lang="scss">
    :global(.bx--accordion__content) {
        padding-right: 0;
    }
</style>