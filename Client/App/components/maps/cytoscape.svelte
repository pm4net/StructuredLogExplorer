<script lang="ts">
    import { createEventDispatcher, onMount } from "svelte";
    import { Button, Search } from "carbon-components-svelte";
    import { Save } from "carbon-icons-svelte";
    import Color from "color";
    import { getColor } from "../../helpers/color-helpers";
    import { placeAroundMatches } from "../../helpers/string-helpers";
    import { DisplayMethod, activeProject, mapSettings } from "../../shared/stores";
    import cytoscape from "cytoscape";
    import nodeHtmlLabel from "cytoscape-node-html-label";
    import viewUtilities from "cytoscape-view-utilities";
    import { BubbleSetsPlugin } from "cytoscape-bubblesets";
    import { logLevelName, logLevelToColor, saveGraphAsImage, zoomToNodes } from "../../helpers/cytoscape-helpers";
    import { Event, type EdgeTypeInfoOfEdgeInfo, type GraphLayout, type ValueTupleOfOcelObjectAndIEnumerableOfValueTupleOfStringAndOcelEvent, OcelObject, ValueTupleOfStringAndOcelEvent, LogLevel, Size } from "../../shared/pm4net-client";
    import { initializeCytoscape } from "../../helpers/cytoscape-layout-helpers";
    import { getStringValue } from "../../helpers/ocel-helpers";
    import ReplayControl from "../replay-control.svelte";
    import ReplayControlDate from "../replay-control-date.svelte";
    import { getLines } from "../../shared/helpers";

    // Props to pass in either a fully defined graph layout or only an OC-DFG, in which case the default layout algorithm will be used.
    export let layout : GraphLayout | undefined = undefined;
    export let ocdfg : GraphLayout | undefined = undefined;
    export let layoutEngine : DisplayMethod | undefined = undefined;
    $: hasLayout = layout !== undefined;

    // Create event dispatcher
    const dispatch = createEventDispatcher();

    // Create cytoscape instance and register extensions
    let cy : cytoscape.Core;
    nodeHtmlLabel(cytoscape);
    viewUtilities(cytoscape);

    // State values
    let searchVal : string;
    let viewUtilitiesApi : any;

    let stateTraces : ValueTupleOfOcelObjectAndIEnumerableOfValueTupleOfStringAndOcelEvent[] | null;
    let stateTrace : { item1: OcelObject, item2: ValueTupleOfStringAndOcelEvent[], text: string } | null;
    let showMultipleTraces = false;
    let showSingleTrace = false;

    // To remove transition class from previous step
    let previouslyAnimatedNode : cytoscape.Collection | undefined;
    let previouslyAnimatedEdge : cytoscape.SingularElementArgument | undefined;
    let previouslyAnimatedEdgeWidth : any;
    let previouslyAnimatedLevelName : string | undefined;

    // Highlight the nodes and edges that are present in a list of traces
    export function highlightTraces(traces: ValueTupleOfOcelObjectAndIEnumerableOfValueTupleOfStringAndOcelEvent[]) {
        if (traces.length > 0) {
            stateTraces = traces;
            showMultipleTraces = true;
            showSingleTrace = false;
        } else {
            stateTraces = null;
            showMultipleTraces = false;
            showSingleTrace = false;
        }

        // First reset all highlights that were added previously
        viewUtilitiesApi.removeHighlights(cy.elements());
        cy.nodes().forEach(n => { n.data('disabled', false) });
        cy.nodes().forEach(n => { n.data('slightlyHidden', false) });
        cy.nodes().removeData("traceText");

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
        if (trace != null) {
            stateTrace = trace;
            showMultipleTraces = false;
            showSingleTrace = true;
        } else {
            stateTrace = null;
            showMultipleTraces = true;
            showSingleTrace = false;
        }

        // Find start and end node
        let type = trace?.item1.type;
        let startNode = cy.$id(`ProcessGraphLayout_Start-${type}`);
        let endNode = cy.$id(`ProcessGraphLayout_End-${type}`);

        // Find elements that have not been disabled before (meaning they are not part of any of the traces for the object type)
        let active = cy.elements().filter(e => e.data('disabled') === false).add(startNode).add(endNode);

        // First reset all highlights that were added previously (by this function, not the other)
        active.forEach(n => { n.data('slightlyHidden', false) });
        active.removeData("traceText"); // Remove previously assigned trace text from all ndoes
        viewUtilitiesApi.removeHighlights(active);
        viewUtilitiesApi.removeHighlights(active.connectedEdges());
        
        if (trace !== null) {
            // Get the list of activities that are present in the trace
            let traceNodeNames = trace.item2.map(event => event.item2.activity);
            traceNodeNames.unshift(startNode.id());
            traceNodeNames.push(endNode.id());

            // Find the nodes and edges that should be slightly greyed out
            let nodesToHide = active.filter(n => !traceNodeNames.includes(n.id())).subtract(startNode).subtract(endNode);
            let edgesToHide = active.connectedEdges().filter(e => {
                let typeInfos : EdgeTypeInfoOfEdgeInfo[] = e.data('typeInfos');
                let isCorrectType = typeInfos.some(t => t.type === type);

                // Figure out whether edge belongs to the trace
                let sourceId = e.source().id();
                let targetId = e.target().id();
                let belongsToTrace = false;

                // Loop through trace and check whether the source and target occur in it, directly following each other
                for (let i = 0; i < traceNodeNames.length; i++) {
                    const elem = traceNodeNames[i];

                    // If the current element isn't the source ID, it is not relevant for this iteration.
                    if (elem !== sourceId) {
                        continue;
                    }

                    // Still has a next element?
                    if (i < traceNodeNames.length - 1) {
                        if (traceNodeNames[i + 1] === targetId) {
                            belongsToTrace = true;
                        } else {
                            continue;
                        }
                    } else {
                        continue;
                    }
                }

                return !isCorrectType || !belongsToTrace;
            });
            let elemsToHide = nodesToHide.union(edgesToHide);

            // Update disabled field on nodes to ensure style updating of HTML labels
            nodesToHide.forEach(n => { n.data('slightlyHidden', true) });

            // Find all nodes and edges that remain
            let elemsToHighlight = active.subtract(elemsToHide);

            // Reverse to make the "first" occurrence of the same event be shown, since it sets the text last (but using slice first to avoid mutation of trace)
            // During animation, text should be swapped out when there's multiple instances, right after passing through it
            trace?.item2.slice().reverse().forEach(event => {
                let cyNode = cy.$id(event.item2.activity).first();
                cyNode.data("traceText", getStringValue(event.item2.vMap["pm4net_RenderedMessage"]));
            });
            
            // Hide the elements that aren't part of the trace, and zoom to the ones remaining
            viewUtilitiesApi.highlight(elemsToHide, 1);
            viewUtilitiesApi.zoomToSelected(elemsToHighlight);
        } else {
            viewUtilitiesApi.zoomToSelected(cy.elements());
        }
    }

    function animateSpecificTrace(index: number) {
        let fromEvent = stateTrace?.item2[index];
        let toEvent = index < ((stateTrace?.item2.length ?? 0) - 1) ? stateTrace?.item2[index + 1] : undefined;

        if (previouslyAnimatedNode && previouslyAnimatedLevelName) {
            previouslyAnimatedNode.removeClass(previouslyAnimatedLevelName);
            previouslyAnimatedNode.addClass(previouslyAnimatedLevelName + "-node-remove-highlight");
        }

        if (previouslyAnimatedEdge) {
            //previouslyAnimatedEdge.removeClass("edge-highlighted");
            previouslyAnimatedEdge.style("width", previouslyAnimatedEdgeWidth);
        }

        if (fromEvent && toEvent) {
            let fromNode = cy.$id(fromEvent.item2.activity);
            let toNode = cy.$id(toEvent.item2.activity);
            let connectingEdge = fromNode
                .edgesTo(toNode)
                .filter(e => e.data("typeInfos").some((ti: EdgeTypeInfoOfEdgeInfo) => ti.type === stateTrace?.item1.type))
                .first();

            let logLevel = fromNode.data("info")?.logLevel as LogLevel;
            let levelName = logLevelName(logLevel);

            if (levelName) {
                fromNode.removeClass(levelName + "-node-remove-highlight"); // Just in case it is still set from a previous animation
                fromNode.addClass(levelName + "-node-highlighted");
                previouslyAnimatedNode = fromNode;
                previouslyAnimatedLevelName = levelName;
            }

            //connectingEdge.addClass("edge-highlighted");
            previouslyAnimatedEdge = connectingEdge;
            previouslyAnimatedEdgeWidth = connectingEdge.style("width");
            connectingEdge.style("width", "10px");
        }

        // If the previous node appears again later in the trace, the text on it should be updated to that of the next occurrence
        let traceNodes = cy.nodes().filter(n => n.data("traceText") !== undefined);
        traceNodes.forEach(n => {
            let nextOccurrence = stateTrace?.item2.slice(index).find(node => node.item2.activity === n.id());
            if (nextOccurrence) {
                n.data("traceText", getStringValue(nextOccurrence.item2.vMap["pm4net_RenderedMessage"]));
            }
        });

        // Highlight the event in the trace window
        dispatch("animateStepChanged", index);
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
                    let nodeSize = data.size as Size;
                    let wrapped = getLines(document.getElementsByTagName("canvas")?.[0] || document.createElement("canvas"), data.traceText, nodeSize.width / 2, nodeSize.height / 2);                    
                    text = wrapped.filter((l: string) => l).join('<br>');
                } else {
                    text = data.text.filter((l: string) => l).join('<br>');
                    text = placeAroundMatches(text, '{', '}', '<strong>', '</strong>');
                }

                let opacity = 1;
                if (data.disabled) { opacity = 0.33; }
                if (data.slightlyHidden) { opacity = 0.66; }

                return `<span style="color: rgba(${txtColor.red()}, ${txtColor.green()}, ${txtColor.blue()}, ${opacity})">${text}</span>`
            }
        }]);

        // Initialize view utilities extension
        var options = {
            highlightStyles: [
                { node: { 'opacity': 0.33 }, edge: { 'opacity': 0.33 } }, // Inactive
                { node: { 'opacity': 0.66 }, edge: { 'opacity': 0.66 } }, // Active, but not part of current trace
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
                        bb.addPath(matchingNodes, matchingNodes.connectedEdges(), null, { // matchingNodes.edgesWith(matchingNodes) yields no rendered sets at all
                            style: {
                                'stroke': 'black',
                                'fill': getColor(ns),
                                'fillOpacity': '0.25'
                            }
                        });
                    }
                });

                // console.log(bb.getPaths());
            });
        }
    });

    function getMinDateInAllTraces(traces: ValueTupleOfOcelObjectAndIEnumerableOfValueTupleOfStringAndOcelEvent[]) {
        return new Date(); // TODO
    }

    function getMaxDateInAllTraces(traces: ValueTupleOfOcelObjectAndIEnumerableOfValueTupleOfStringAndOcelEvent[]) {
        return new Date(); // TODO
    }
</script>

<Search placeholder="Search nodes..." bind:value={searchVal} on:change={(_) => zoomToNodes(cy, viewUtilitiesApi, searchVal)}></Search>
<div class="save-btn">
    <Button 
        kind="secondary" 
        iconDescription="Save image" 
        icon={Save} 
        tooltipPosition="left" 
        on:click={((_) => saveGraphAsImage(cy, $activeProject ?? ""))}>
    </Button>
</div>
<div id="dfg"></div>

{#if showMultipleTraces && stateTraces}
    <!-- TODO: Replay control that replays all traces of object type simultaneously -->
    <ReplayControlDate
        min={getMinDateInAllTraces(stateTraces)} 
        max={getMaxDateInAllTraces(stateTraces)}>
    </ReplayControlDate>
{:else if showSingleTrace && stateTrace}
    <ReplayControl
        min={1}
        max={stateTrace.item2.length}
        on:sliderChange={(v) => animateSpecificTrace(v.detail - 1)}>
    </ReplayControl>
{/if}

<style lang="scss">
    #dfg {
        width: 100%;
        height: calc(100vh - 48px);
    }

    .save-btn {
        position: absolute;
        z-index: 1;
        top: 1rem;
        right: 1rem;
    }

    :global(.bx--search) {
        position: absolute;
        z-index: 1;
        top: 1rem;
        left: 1rem;
        width: calc(100% - 6rem);
    }

    :global(.cy-title) {
        text-align: center;
    }
</style>