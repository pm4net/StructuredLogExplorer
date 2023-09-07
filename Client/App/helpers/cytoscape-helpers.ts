import { saveAs } from "file-saver";
import { Edge, LogLevel } from "../shared/pm4net-client";

export function zoomToNodes(cy: cytoscape.Core, viewUtilitiesApi: any, search: string) {
    // Find nodes that match the search query
    let matchingNodes = cy.nodes().filter(function(el) {
        return el.data('text').join(' ').toLowerCase().includes(search.toLowerCase());
    });

    // Find edges that connect the matching nodes and take the union with the nodes
    let connectingEdges = matchingNodes.edgesWith(matchingNodes);
    let eles = matchingNodes.union(connectingEdges);

    // Zoom the matching nodes into view
    viewUtilitiesApi.zoomToSelected(eles);
}

export async function saveGraphAsImage(cy: cytoscape.Core, activeProject: string) {
    let pngBlob = await cy.png({
        output: 'blob-promise',
        full: true
    });
    saveAs(pngBlob, `${activeProject}.png`);
}

export function logLevelFromString(level: string) {
    switch (level) {
        case "Verbose":
            return LogLevel.Verbose;
        case "Debug":
            return LogLevel.Debug;
        case "Information":
            return LogLevel.Information;
        case "Warning":
            return LogLevel.Warning;
        case "Error":
            return LogLevel.Error;
        case "Fatal":
            return LogLevel.Fatal;
        default:
            return LogLevel.Unknown;
    }
}

export function logLevelToColor(level: LogLevel) {
    switch (level) {
        case LogLevel.Verbose:
            return "#FFFFFF";
        case LogLevel.Debug:
            return "#E0E0E0";
        case LogLevel.Information:
            return "#C0C0C0";
        case LogLevel.Warning:
            return "#FF9933";
        case LogLevel.Error:
            return "#FF3333";
        case LogLevel.Fatal:
            return "#990000";
        default:
            return "#FFFFFF";
    }
}

export function logLevelName(level: LogLevel) {
    switch (level) {
        case LogLevel.Verbose: 
            return "verbose";
        case LogLevel.Debug: 
            return "debug";
        case LogLevel.Information: 
            return "info";
        case LogLevel.Warning: 
            return "warning";
        case LogLevel.Error: 
            return "error";
        case LogLevel.Fatal: 
            return "fatal";
        case LogLevel.Unknown: 
            return undefined;
    }
}

export function getEdgeId(edge: Edge) {
    return edge.sourceId + "-" + edge.targetId;
}