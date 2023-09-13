<script lang="ts">
    import { Accordion, AccordionItem, Button, ButtonSet, Checkbox, DatePicker, DatePickerInput, FormGroup, NumberInput, RadioButton, RadioButtonGroup } from "carbon-components-svelte";
    import { activeProject, DisplayMethod, DisplayType, EdgeType, mapSettings } from "../shared/stores";
    import { getColor } from "../helpers/color-helpers";
    import { onMount } from "svelte";
    import { KeepCases, LogLevel } from "../shared/pm4net-client";

    export let availableObjectTypes = <string[]>[];
    export let highLevelNamespaces = <string[]>[];
    export let minDate : string;
    export let maxDate : string;
    
    let displayType = $mapSettings[$activeProject ?? ""]?.displayType;
    let edgeType = $mapSettings[$activeProject ?? ""]?.edgeType;
    let displayMethod = $mapSettings[$activeProject ?? ""]?.displayMethod;
    let groupByNamespace = $mapSettings[$activeProject ?? ""]?.groupByNamespace;
    let objectTypes = $mapSettings[$activeProject ?? ""]?.objectTypes ?? [];
    let logLevels = $mapSettings[$activeProject ?? ""]?.logLevels ?? [];
    let namespaces = $mapSettings[$activeProject ?? ""]?.namespaces ?? [];
    let fixUnforeseenEdges = $mapSettings[$activeProject ?? ""].fixUnforeseenEdges;
    let minEvents = $mapSettings[$activeProject ?? ""].dfg.minEvents;
    let minOccurrences = $mapSettings[$activeProject ?? ""].dfg.minOccurrences;
    let minSuccessions = $mapSettings[$activeProject ?? ""].dfg.minSuccessions;
    let dateFrom = $mapSettings[$activeProject ?? ""].dfg.dateFrom ?? minDate;
    let dateTo = $mapSettings[$activeProject ?? ""].dfg.dateTo ?? maxDate;
    let keepCases = $mapSettings[$activeProject ?? ""].dfg.keepCases;

    $: {
        let settings = $mapSettings;
        settings[$activeProject ?? ""].displayType = displayType;
        settings[$activeProject ?? ""].edgeType = edgeType;
        settings[$activeProject ?? ""].displayMethod = displayMethod;
        settings[$activeProject ?? ""].groupByNamespace = groupByNamespace;
        settings[$activeProject ?? ""].objectTypes = objectTypes;
        settings[$activeProject ?? ""].logLevels = logLevels;
        settings[$activeProject ?? ""].namespaces = namespaces;
        settings[$activeProject ?? ""].fixUnforeseenEdges = fixUnforeseenEdges;
        settings[$activeProject ?? ""].dfg.minEvents = minEvents;
        settings[$activeProject ?? ""].dfg.minOccurrences = minOccurrences;
        settings[$activeProject ?? ""].dfg.minSuccessions = minSuccessions;
        settings[$activeProject ?? ""].dfg.dateFrom = dateFrom;
        settings[$activeProject ?? ""].dfg.dateTo = dateTo;
        settings[$activeProject ?? ""].dfg.keepCases = keepCases;
        mapSettings.set(settings);
    }

    // Resize observer
    let filterElem : Element;
    let filterWidth : number;

    onMount(() => {
        // Register resize observer and change width whenever it changes
        const resizeObserver = new ResizeObserver(entries => {
            const entry = entries.at(0);
            filterWidth = entry?.contentBoxSize[0].inlineSize!;
        });
        resizeObserver.observe(filterElem);
        return () => resizeObserver.unobserve(filterElem);
    });

    // Just get the damn number, it is already...
    function getNum(val: string | LogLevel) : number {
        return val as number;
    }
</script>

<div bind:this={filterElem}>
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
                    <RadioButton labelText="MSAGL" value={DisplayMethod.Msagl}></RadioButton>
                    <RadioButton labelText="Cytoscape" value={DisplayMethod.Cytoscape} />
                    <RadioButton labelText="Cytoscape BFS" value={DisplayMethod.CytoscapeBfs} />
                    <RadioButton labelText="Cytoscape Cose" value={DisplayMethod.CytoscapeCose} />
                </RadioButtonGroup>
            </FormGroup>
            <FormGroup legendText="Other" noMargin>
                <Checkbox value="groupByNamespace" labelText="Group by namespace" bind:checked={groupByNamespace}></Checkbox>
                <Checkbox value="fixUnforeseenEdges" labelText="Fix unforeseen edges" bind:checked={fixUnforeseenEdges}></Checkbox>
            </FormGroup>
        </AccordionItem>
        <AccordionItem>
            <svelte:fragment slot="title"><strong>Frequency</strong></svelte:fragment>
            <NumberInput label="Min. events in trace" min={0} bind:value={minEvents} />
            <NumberInput label="Min. occurrences" min={0} bind:value={minOccurrences} />
            <NumberInput label="Min. successions" min={0} bind:value={minSuccessions} />
        </AccordionItem>
        <AccordionItem>
            <svelte:fragment slot="title"><strong>Timeframe</strong></svelte:fragment>
            <DatePicker datePickerType="range" dateFormat="Y/m/d" minDate={minDate} maxDate={maxDate} bind:valueFrom={dateFrom} bind:valueTo={dateTo}>
                <DatePickerInput labelText="From" placeholder="yyyy/mm/dd"></DatePickerInput>
                <DatePickerInput labelText="To" placeholder="yyyy/mm/dd"></DatePickerInput>
            </DatePicker>
            <RadioButtonGroup orientation="vertical" legendText="Keep cases" bind:selected={keepCases}>
                <RadioButton labelText="Contained in timeframe" value={KeepCases.ContainedInTimeFrame}></RadioButton>
                <RadioButton labelText="Intersecting timeframe" value={KeepCases.IntersectingTimeFrame}></RadioButton>
                <RadioButton labelText="Started in timeframe" value={KeepCases.StartedInTimeFrame}></RadioButton>
                <RadioButton labelText="Completed in timeframe" value={KeepCases.CompletedInTimeFrame}></RadioButton>
                <RadioButton labelText="Trim to timeframe" value={KeepCases.TrimToTimeFrame}></RadioButton>
            </RadioButtonGroup>
        </AccordionItem>
        <AccordionItem>
            <svelte:fragment slot="title"><strong>Log levels</strong></svelte:fragment>
            {#each Object.values(LogLevel).filter(v => !isNaN(Number(v))) as logLevel}
                <Checkbox bind:group={logLevels} value={logLevel}>
                    <svelte:fragment slot="labelText">{LogLevel[getNum(logLevel)]}</svelte:fragment>
                </Checkbox>
            {/each}
        </AccordionItem>
        <AccordionItem>
            <svelte:fragment slot="title"><strong>Object types</strong></svelte:fragment>
            <ButtonSet stacked={filterWidth < 425}>
                <Button size="small" kind="tertiary" on:click={() => (objectTypes = availableObjectTypes)}>Select all</Button>
                <Button size="small" kind="tertiary" on:click={() => (objectTypes = [])}>Deselect all</Button>
            </ButtonSet>
            {#each availableObjectTypes as objType}
                <Checkbox bind:group={objectTypes} value={objType}>
                    <svelte:fragment slot="labelText">{objType} <div class="circle" style:background-color="{getColor(objType)}"></div></svelte:fragment>
                </Checkbox>
            {/each}
        </AccordionItem>
        <AccordionItem>
            <svelte:fragment slot="title"><strong>Namespaces</strong></svelte:fragment>
            {#each highLevelNamespaces as namespace}
                <Checkbox bind:group={namespaces} value={namespace}>
                    <svelte:fragment slot="labelText">{namespace} <div class="circle" style:background-color="{getColor(namespace)}"></svelte:fragment>
                </Checkbox>
            {/each}
        </AccordionItem>
    </Accordion>
</div>

<style lang="scss">
    :global(.bx--accordion__content) {
        padding-right: 0;
    }

    :global(.bx--number) {
        padding-right: 1rem;
    }

    :global(.bx--checkbox-label) {
        width: 100%;
    }

    :global(.bx--checkbox-label-text) {
       flex-grow: 1;
    }

    :global(.bx--btn-set) {
        justify-content: center;
        align-items: center;
        padding-bottom: 1rem;
    }

    .circle {
        height: 16px;
        width: 16px;
        border-radius: 50%;
        margin-right: 1rem;
        float: right;
    }
</style>