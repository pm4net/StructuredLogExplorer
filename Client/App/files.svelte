<script lang="ts">
    import { DateTime } from "luxon";
    import prettyBytes from 'pretty-bytes';

    import Layout from "./shared/layout.svelte";
    import { Upload, Renew } from "carbon-icons-svelte";
    import { Button, DataTable, DataTableSkeleton, InlineLoading, InlineNotification, Pagination, Row, Toolbar, ToolbarContent, ToolbarSearch } from "carbon-components-svelte";

    import { activeProject } from "./shared/stores";
    import { filesClient } from "./shared/pm4net-client-config";

    let pagination = { pageSize: 10, page: 1, filteredRowIds: <number[]>[] }

    // Load log files of active project from API
    let files = filesClient.getLogFileInfos($activeProject);
    let importingFiles = <string[]>[]

    // Format a Luxon DateTime in a human-friendly way
    function formatDateTime(dt: any) {
        if (dt instanceof DateTime) {
            return dt.toFormat("yyyy-MM-dd HH:mm");
        }

        return "";
    }

    // Import all log files
    async function importAll(fileNames: string[]) {
        try {
            importingFiles = importingFiles.concat(fileNames);

            // Replace all entries in the file list with a new Promise that contains the returned results
            let infos = await filesClient.importAll($activeProject);
            if (infos) {
                files = Promise.resolve(Object.values(infos));
            }

            importingFiles = [];
        } catch (e: unknown) {
            // TODO: Show modal with error message
            console.error(e);
        }
    }

    // Import a specific log file
    async function importLog(name: string) {
        try {
            importingFiles.push(name);
            importingFiles = importingFiles;

            // Get the current list of files by awaiting the promise, and then create a new Promise with the updated result
            let filesBeforeImport = await files;
            let logFileInfo = await filesClient.importLog($activeProject, name);
            if (logFileInfo) {
                let idx = filesBeforeImport.findIndex(f => f.id === name);
                filesBeforeImport[idx] = logFileInfo;
                files = Promise.resolve(filesBeforeImport);
            }

            importingFiles.splice(importingFiles.indexOf(name), 1);
            importingFiles = importingFiles;
        } catch (e: unknown) {
            // TODO: Show modal with error message
            console.error(e);
        }
    }
</script>

<Layout>
    {#if $activeProject}
        {#await files}
            <DataTableSkeleton />
        {:then files}
            <DataTable
                sortable
                sortKey="lastChanged"
                title="Log files" 
                description="Overview of the log files in the project's log directory and their sync status."
                headers={[
                    { key: "id", value: "Name" },
                    { key: "noOfImportedEvents", value: "Imported events" },
                    { key: "noOfImportedObjects", value: "Imported objects" },
                    { key: "fileSize", value: "File size" },
                    { key: "lastImported", value: "Last imported" },
                    { key: "lastChanged", value: "Last changed" },
                    { key: "overflow", empty: true }
                ]}
                rows={files}
                pageSize={pagination.pageSize}
                page={pagination.page}>

                <Toolbar>
                    <ToolbarContent>
                        <ToolbarSearch 
                            persistent 
                            bind:filteredRowIds={pagination.filteredRowIds}
                            shouldFilterRows={(row, value) => {
                                if (typeof value === "string") {
                                    return row["id"].toLowerCase().includes(value.toLowerCase()); 
                                }
                                return false;
                            }}
                        />
                        <Button icon={Renew} on:click={() => importAll(files.map(f => f.id))}>Import or refresh all</Button>
                    </ToolbarContent>
                </Toolbar>

                <svelte:fragment slot="cell" let:row let:cell>
                    {#if cell.key === "overflow"}
                        {#if importingFiles.includes(row.id)}
                            <InlineLoading status="active" description="Importing file..." />
                        {:else}
                            {#if row["lastImported"] === undefined}
                                <Button icon={Upload} on:click={() => importLog(row.id)}>Import</Button>
                            {:else if row["lastChanged"] > row["lastImported"]}
                                <Button icon={Renew} on:click={() => importLog(row.id)}>Refresh</Button>
                            {:else}
                                <InlineLoading status="finished" description="All up-to date!" />
                            {/if}
                        {/if}
                    {:else if cell.key === "fileSize"}
                        {prettyBytes(cell.value)}
                    {:else if cell.key === "lastImported" || cell.key === "lastChanged"}
                        {formatDateTime(cell.value)}
                    {:else if cell.key === "noOfImportedEvents" || cell.key === "noOfImportedObjects"}
                        {cell.value.toLocaleString()}
                    {:else}
                        {cell.value}
                    {/if}
                </svelte:fragment>
            </DataTable>

            <Pagination
                bind:pageSize={pagination.pageSize}
                bind:page={pagination.page}
                totalItems={pagination.filteredRowIds.length}
                pageSizeInputDisabled
            />
        {:catch error}
            <InlineNotification
                title="Error while fetching file information:"
                subtitle={error}
                lowContrast />

            <DataTableSkeleton />
        {/await}
    {:else}
        <InlineNotification
            title="Please activate a project before visiting this page."
            subtitle="Your log files will then be shown here."
            lowContrast />

        <DataTableSkeleton />
    {/if}
</Layout>
