<script lang="ts">
    import { DateTime } from "luxon";
    import prettyBytes from 'pretty-bytes';

    import Layout from "./shared/layout.svelte";
    import { Button, DataTable, DataTableSkeleton, InlineNotification, Pagination, Row, Toolbar, ToolbarContent, ToolbarSearch } from "carbon-components-svelte";

    import { activeProject } from "./shared/stores";
    import { filesClient } from "./shared/api-clients";

    let pagination = { pageSize: 10, page: 1, filteredRowIds: <number[]>[] }

    // Load log files of active project from API
    let files = filesClient.getLogFileInfos($activeProject);

    // Format a Luxon DateTime in a human-friendly way
    function formatDateTime(dt: any) {
        if (dt instanceof DateTime) {
            return dt.toFormat("yyyy-MM-dd HH:mm");
        }

        return "";
    }
</script>

<Layout>
    {#if $activeProject}
        {#await files}
            <DataTableSkeleton />
        {:then files}
            <DataTable 
                sortable
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
                    </ToolbarContent>
                </Toolbar>

                <svelte:fragment slot="cell" let:row let:cell>
                    {#if cell.key === "overflow"}
                        <Button disabled>Import</Button>
                    {:else if cell.key === "fileSize"}
                        {prettyBytes(cell.value)}
                    {:else if cell.key === "lastImported" || cell.key === "lastChanged"}
                        {formatDateTime(cell.value)}
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
