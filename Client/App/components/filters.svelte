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
    <AccordionItem>
        

Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam hendrerit risus at magna posuere pharetra. Duis interdum quis lorem eget fringilla. Curabitur vel sapien tortor. Donec aliquam malesuada tempor. Cras scelerisque blandit tristique. Quisque accumsan mi id lectus ultricies, porta mollis elit scelerisque. Nunc viverra purus velit, ut dignissim nunc facilisis venenatis. Aenean porta orci eleifend leo aliquam, quis gravida orci tincidunt.

Duis sollicitudin quis odio sit amet blandit. Donec efficitur, eros ac molestie rutrum, arcu mauris varius sapien, vitae porttitor turpis nisl faucibus ex. Cras sit amet vulputate arcu. Donec ipsum nunc, sagittis quis leo at, dignissim commodo sem. Morbi at leo ultrices, ultrices quam feugiat, consequat orci. Pellentesque dolor nulla, tempor at bibendum vitae, suscipit sit amet nisl. Mauris posuere tortor id metus posuere, vel rhoncus dolor consequat. Vestibulum suscipit congue libero id tincidunt. Aliquam erat volutpat. Pellentesque sed ornare tortor. In sollicitudin quam orci, egestas faucibus magna iaculis ut. Quisque at urna finibus leo ultricies cursus. Phasellus sollicitudin elit tellus, at rutrum nulla accumsan ac. Etiam quis fringilla nulla, pharetra tincidunt ligula.

Praesent sed nisl magna. Sed sodales in elit at tempor. Maecenas convallis, justo ac malesuada consequat, urna enim sagittis metus, ac egestas arcu nunc sed massa. Nunc porta ligula eros, ut vestibulum velit luctus quis. Vestibulum gravida, neque id eleifend eleifend, ipsum neque convallis diam, vel vulputate felis elit ultricies nisl. Praesent nunc metus, aliquam ac mattis id, scelerisque et arcu. Aenean semper purus at dolor malesuada iaculis. Nulla finibus rhoncus lectus, eu porttitor tellus consequat fringilla. Phasellus tristique cursus sapien quis congue. Duis eget erat tempus risus finibus convallis sit amet at ligula. Donec urna tortor, luctus eu ultricies quis, elementum at augue. Donec cursus semper ultricies. Quisque posuere pulvinar nulla, id rhoncus mauris ultrices consectetur.

Ut non lacus laoreet, rhoncus arcu id, mollis leo. Duis sagittis risus eu diam maximus imperdiet. Morbi eu eros fringilla purus posuere molestie. Mauris vitae urna velit. Donec sed urna egestas, facilisis lorem quis, luctus elit. Suspendisse sit amet viverra ipsum, quis volutpat nibh. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer hendrerit, velit vel tempor vehicula, quam libero interdum lacus, scelerisque sagittis dui metus porttitor mauris. Phasellus sed tempor leo. Cras lacus ex, ultrices quis faucibus id, sollicitudin id nisi. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Mauris bibendum neque in tempor gravida. Morbi vel consectetur elit.

Phasellus nisi ipsum, porttitor at condimentum vel, iaculis quis felis. Aenean id sollicitudin sem, et semper velit. Etiam ut sem lobortis mi lobortis tempor ac at nisi. Ut dictum, turpis eget ultrices commodo, ante diam ornare erat, nec rutrum sem dui eget orci. Donec vel dapibus nisi. Pellentesque tincidunt purus at erat condimentum sollicitudin sit amet nec libero. Mauris velit nunc, ultrices quis ante quis, maximus eleifend dui. Curabitur nec leo nec ante posuere fermentum sit amet vel felis. In dictum ligula eget viverra ornare. Integer tristique ante scelerisque metus porttitor accumsan. Phasellus dapibus luctus massa, nec ornare urna interdum eget. Nunc congue lobortis est nec malesuada. Nulla nec orci sollicitudin, molestie purus a, interdum dui. Curabitur et dui nec libero facilisis mattis. Suspendisse eros quam, sodales quis lacus nec, vehicula maximus enim. Donec ut pretium sem. 
    </AccordionItem>
</Accordion>