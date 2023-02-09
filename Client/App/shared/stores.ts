import { persisted } from "svelte-local-storage-store";

export const activeProject = persisted<string | null>("activeProject", null);
export const mapSettings = persisted<ProjectMapSettings>("mapSettings", {});

export type ProjectMapSettings = { [id: string] : MapSettings }
export type MapSettings = {
    displayType: DisplayType,
    edgeType: EdgeType,
    displayMethod: DisplayMethod,
    groupByNamespace: boolean,
    objectTypes: string[],
    dfg: {
        minEvents: number,
        minOccurrences: number,
        minSuccessions: number
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
    Custom = "custom"
}