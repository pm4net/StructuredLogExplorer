import { persisted } from "svelte-local-storage-store";
import type { KeepCases } from "./pm4net-client";

export const activeProject = persisted<string | undefined>("activeProject", undefined);
export const mapSettings = persisted<ProjectMapSettings>("mapSettings", {});

export type ProjectMapSettings = { [id: string] : MapSettings }
export type MapSettings = {
    displayType: DisplayType,
    edgeType: EdgeType,
    displayMethod: DisplayMethod,
    groupByNamespace: boolean,
    objectTypes: string[],
    fixUnforeseenEdges: boolean,
    dfg: {
        minEvents: number,
        minOccurrences: number,
        minSuccessions: number,
        dateFrom?: string,
        dateTo?: string,
        keepCases: KeepCases
    }
}

export enum DisplayType {
    OcDfg = "ocdfg",
    OcPn = "ocpn"
}

export enum EdgeType {
    Frequency = "frequency",
    Performance = "performance"
}

export enum DisplayMethod {
    Dot = "dot",
    Cytoscape = "cytoscape",
    CytoscapeBfs = "cytoscape-bfs",
    CytoscapeCose = "cytoscape-cose",
    CytoscapeDagre = "cytoscape-dagre"
}