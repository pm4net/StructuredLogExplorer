<script lang="ts">
    import { onMount } from "svelte";
    import { Button, Search } from "carbon-components-svelte";
    import { Save } from "carbon-icons-svelte";
    import Color from "color";
    import { getColor } from "../../helpers/color-helpers";
    import { placeAroundMatches } from "../../helpers/string-helpers";
    import { DisplayMethod, activeProject, mapSettings } from "../../shared/stores";
    import cytoscape, { type NodeCollection, type NodeSingular } from "cytoscape";
    import nodeHtmlLabel from "cytoscape-node-html-label";
    import viewUtilities from "cytoscape-view-utilities";
    import { BubbleSetsPlugin } from "cytoscape-bubblesets";
    import { logLevelToColor, resetHighlights, saveGraphAsImage, zoomToNodes } from "../../helpers/cytoscape-helpers";
    import { Event, type EdgeTypeInfoOfEdgeInfo, type GraphLayout, type ValueTupleOfOcelObjectAndIEnumerableOfValueTupleOfStringAndOcelEvent, OcelObject, ValueTupleOfStringAndOcelEvent } from "../../shared/pm4net-client";
    import { initializeCytoscape } from "../../helpers/cytoscape-layout-helpers";
    import { getStringValue } from "../../helpers/ocel-helpers";

    // Props to pass in either a fully defined graph layout or only an OC-DFG, in which case the default layout algorithm will be used.
    export let layout : GraphLayout | undefined = undefined;
    export let ocdfg : GraphLayout | undefined = undefined;
    export let layoutEngine : DisplayMethod | undefined = undefined;
    $: hasLayout = layout !== undefined;

    // Create cytoscape instance and register extensions
    let cy : cytoscape.Core;
    nodeHtmlLabel(cytoscape);
    viewUtilities(cytoscape);

    // State values
    let searchVal : string;
    let viewUtilitiesApi : any;

    // Highlight the nodes and edges that are present in a list of traces
    export function highlightTraces(traces: ValueTupleOfOcelObjectAndIEnumerableOfValueTupleOfStringAndOcelEvent[]) {
        // First reset all highlights that were added previously
        resetHighlights(cy, viewUtilitiesApi);
        cy.nodes().forEach(n => { n.data('disabled', false) });

        if (traces.length > 0) {
            // Get set of nodes that are present in the traces
            let nodes = new Set(traces.flatMap(trace => trace.item2.map(event => event.item2.activity)));

            // Find start and end node
            let type = traces.length > 0 ? traces[0].item1.type : undefined;
            let startNode = cy.$id(`ProcessGraphLayout_Start-${type}`);
            let endNode = cy.$id(`ProcessGraphLayout_End-${type}`);

            // Find the nodes and edges that should be hidden
            let nodesToHide = cy.nodes().filter(n => !nodes.has(n.id())).subtract(startNode).subtract(endNode);
            let edgesToHide = nodesToHide.connectedEdges().filter(e => {
                let typeInfos : EdgeTypeInfoOfEdgeInfo[] = e.data('typeInfos');
                let isCorrectType = typeInfos.some(t => t.type === type);
                return !isCorrectType || !nodes.has(e.source().id()) || !nodes.has(e.target().id());
            });
            let elemsToHide = nodesToHide.union(edgesToHide);

            // Update disabled field on nodes to ensure style updating of HTML labels
            nodesToHide.forEach(n => { n.data('disabled', true) });

            // Find all nodes and edges that remain
            let elemsToHighlight = elemsToHide.absoluteComplement().union(startNode).union(endNode);

            // Hide the elements that aren't part of the traces, and zoom to the ones remaining
            viewUtilitiesApi.highlight(elemsToHide, 0);
            viewUtilitiesApi.zoomToSelected(elemsToHighlight);
        } else {
            viewUtilitiesApi.zoomToSelected(cy.elements());
        }
    }

    // Highlight the nodes and edges for a specific trace, replacing the text inside of the nodes with the real rendered text
    export function highlightSpecificTrace(trace: { item1: OcelObject, item2: ValueTupleOfStringAndOcelEvent[], text: string } | null) {
        // First reset all highlights that were added previously (by this function, not the other)
        //let active = cy.elements().filter(e => e.data('disabled') === false);
        //viewUtilitiesApi.removeHighlights(active);
        cy.nodes().removeData("traceText"); // Remove previously assigned trace text from all ndoes
        
        if (trace !== null) {
            let highlightedNodes = cy.collection(); // Empty collection
            trace?.item2.reverse().forEach(event => { // Reverse to make the "first" occurrence of the same event be shown, since it sets the text last
                let cyNode = cy.$id(event.item2.activity).first();
                highlightedNodes = highlightedNodes.add(cyNode);
                cyNode.data("traceText", getStringValue(event.item2.vMap["pm4net_RenderedMessage"]));
            });

            // Find the nodes and edges that should be slightly greyed out
            let nodesToHideSlightly = cy.nodes().difference(highlightedNodes);
            //nodesToHideSlightly.forEach(n => { n.data('disabled', true) });
            //viewUtilitiesApi.highlight(nodesToHideSlightly, 1);
            viewUtilitiesApi.zoomToSelected(highlightedNodes);
        } else {
            viewUtilitiesApi.zoomToSelected(cy.elements());
        }

        // TODO: slighlty reduce opacity of events not in trace (but in object type)
        // TODO: What about events that happen multiple times in a trace?
    }

    onMount(() => {
        cy = initializeCytoscape(hasLayout ? layout! : ocdfg!, hasLayout, layoutEngine, document.getElementById("dfg"));

        // Add HTML labels to nodes
        cy.nodeHtmlLabel([{
            query: 'node',
            halign: 'center', // title vertical position. Can be 'left',''center, 'right'
            valign: 'center', // title vertical position. Can be 'top',''center, 'bottom'
            halignBox: 'center', // title vertical position. Can be 'left',''center, 'right'
            valignBox: 'center', // title relative box vertical position. Can be 'top',''center, 'bottom'
            cssClass: 'cy-title', // any classes will be as attribute of <div> container for every title
            tpl: function(data: any) {

                // Text color calculations
                let bgColor;
                if (data.type instanceof Event) {
                    bgColor = logLevelToColor(data.info?.logLevel);
                } else {
                    bgColor = getColor(data.type.objectType);
                }
                let txtColor = Color(bgColor).isDark() ? Color('#FFFFFF') : Color("#000000");

                // Text calculations
                let text : string;
                if (data.traceText) {
                    text = data.traceText; // TODO: How to do line breaks, overflows, etc.?
                } else {
                    text = data.text.filter((l: string) => l).join('<br>');
                    text = placeAroundMatches(text, '{', '}', '<strong>', '</strong>');
                }

                return `<span style="color: rgba(${txtColor.red()}, ${txtColor.green()}, ${txtColor.blue()}, ${data.disabled ? 0.1 : 1})">${text}</span>`
            }
        }]);

        // Initialize view utilities extension
        var options = {
            highlightStyles: [
                { node: { 'opacity': 0.1 }, edge: { 'opacity': 0.1 } }, // Inactive
                //{ node: { 'opacity': 0.75 }, edge: { 'opacity': 0.75 } }, // Active, but not part of current trace
            ],
            selectStyles: {},
            zoomAnimationDuration: 1000, // default duration for zoom animation speed
        };
        viewUtilitiesApi = cy.viewUtilities(options);

        // Initialize bubble sets
        if ($mapSettings[$activeProject ?? ""].groupByNamespace) {
            cy.ready(() => {
                const bb = new BubbleSetsPlugin(cy); // Initialize plugin

                // Group nodes by top-level namespace
                let topLevelNamespaces = new Set<string>(cy.nodes().map(n => n.data('info')?.namespace?.split('.')[0]));
                topLevelNamespaces.forEach(ns => {
                    if (ns) {
                        // TODO: not all of them showing up? Maybe due to overlaps?
                        let matchingNodes = cy.nodes().filter(n => n.data('info')?.namespace?.split('.')[0] === ns);
                        bb.addPath(matchingNodes, matchingNodes.connectedEdges(), null, {
                            style: {
                                'stroke': 'black',
                                'fill': getColor(ns),
                                'fillOpacity': '0.25'
                            }
                        });
                    }
                });
            });
        }
    });
</script>

<Search placeholder="Search nodes..." bind:value={searchVal} on:change={(_) => zoomToNodes(cy, viewUtilitiesApi, searchVal)}></Search>
<Button kind="secondary" iconDescription="Save image" icon={Save} tooltipPosition="left" on:click={((_) => saveGraphAsImage(cy, $activeProject ?? ""))}></Button>
<div id="dfg"></div>

<style lang="scss">
    #dfg {
        width: 100%;
        height: calc(100vh - 48px);
    }

    :global(.bx--search ) {
        position: absolute;
        z-index: 1;
        top: 1rem;
        left: 1rem;
        width: calc(100% - 6rem);
    }

    :global(.bx--btn.bx--btn--icon-only.bx--tooltip__trigger) {
        position: absolute;
        z-index: 1;
        top: 1rem;
        right: 1rem;
    }

    :global(.cy-title) {
        text-align: center;
    }
</style>