<script lang="ts">
    import { DateTime } from "luxon";
    import prettyBytes from "pretty-bytes";
    import { saveAs } from "file-saver";

    import Layout from "./shared/layout.svelte";
    import { Upload, Renew, Export } from "carbon-icons-svelte";
    import { Button, DataTable, DataTableSkeleton, InlineLoading, InlineNotification, Pagination, Row, ToastNotification, Toolbar, ToolbarContent, ToolbarMenu, ToolbarMenuItem, ToolbarSearch } from "carbon-components-svelte";

    import { activeProject } from "./shared/stores";
    import { filesClient } from "./shared/pm4net-client-config";
    import { getErrorMessage } from "./shared/helpers";

    let pagination = { pageSize: 10, page: 1, filteredRowIds: <number[]>[] }

    // Load log files of active project from API
    let files = filesClient.getLogFileInfos($activeProject);
    let importingFiles = <string[]>[]

    // The state of the error notification that is shown when an API error occurs
    let errorNotification = {
        show: false,
        message: ""
    }

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
        } catch (e: unknown) {
            errorNotification.show = true;
            errorNotification.message = getErrorMessage(e);
        } finally {
            importingFiles = [];
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
        } catch (e: unknown) {
            errorNotification.show = true;
            errorNotification.message = getErrorMessage(e);
        } finally {
            importingFiles.splice(importingFiles.indexOf(name), 1);
            importingFiles = importingFiles;
        }
    }

    // Export the entire project to an OCEL file and save it
    async function exportOcel(format: string) {
        try {
            let file = await filesClient.exportOcel($activeProject, format);
            if (file) {
                saveAs(file.data, file.fileName);
            } else {
                throw new Error("Returned file is undefined.");
            }
        } catch (e: unknown) {
            errorNotification.show = true;
            errorNotification.message = getErrorMessage(e);
        }
    }
</script>

<Layout>
    {#if $activeProject}
        {#await files}
            <DataTableSkeleton />
        {:then files}
            {#if errorNotification.show}
                <ToastNotification
                    title="An error occurred"
                    subtitle={errorNotification.message}
                    kind="error"
                    fullWidth
                    lowContrast
                    on:close={() => errorNotification.message = ""}
                />
            {/if}

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
                        <ToolbarMenu icon={Export}>
                            <ToolbarMenuItem on:click={() => exportOcel("json")}>Export to JSON</ToolbarMenuItem>
                            <ToolbarMenuItem on:click={() => exportOcel("xml")}>Export to XML</ToolbarMenuItem>
                            <ToolbarMenuItem on:click={() => exportOcel("litedb")}>Export to LiteDb</ToolbarMenuItem>
                            <ToolbarMenuItem disabled>Export to XES</ToolbarMenuItem>
                        </ToolbarMenu>
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
