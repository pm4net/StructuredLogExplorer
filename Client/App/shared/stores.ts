import { persisted } from "svelte-local-storage-store";
import type { KeepCases, LogLevel } from "./pm4net-client";

export const activeProject = persisted<string | undefined>("activeProject", undefined);
export const mapSettings = persisted<ProjectMapSettings>("mapSettings", {});

export type ProjectMapSettings = { [id: string] : MapSettings }
export type MapSettings = {
    displayType: DisplayType,
    edgeType: EdgeType,
    displayMethod: DisplayMethod,
    groupByNamespace: boolean,
    objectTypes: string[],
    logLevels: LogLevel[],
    namespaces: string[],
    fixUnforeseenEdges: boolean,
    dfg: {
        minEvents: number,
        minOccurrences: number,
        minSuccessions: number,
        dateFrom?: string,
        timeFrom?: string,
        dateTo?: string,
        timeTo?: string,
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
    Msagl = "msagl",
    Cytoscape = "cytoscape",
    CytoscapeBfs = "cytoscape-bfs",
    CytoscapeCose = "cytoscape-cose"
}