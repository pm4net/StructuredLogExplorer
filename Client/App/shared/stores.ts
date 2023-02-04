import { persisted } from "svelte-local-storage-store";

export const activeProject = persisted<string | null>("activeProject", null);
export const mapSettings = persisted<ProjectMapSettings>("mapSettings", {});

export type ProjectMapSettings = { [id: string] : MapSettings }
export type MapSettings = {
    objectTypes: string[],
    dfg: {
        minEvents: number,
        minOccurrences: number,
        minSuccessions: number
    }
}