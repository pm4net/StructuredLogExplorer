import { persisted } from "svelte-local-storage-store";

export const activeProject = persisted("activeProject", "");