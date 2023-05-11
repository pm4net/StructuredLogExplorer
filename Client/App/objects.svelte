<script lang="ts">
    import { Button, DataTable, DataTableSkeleton, InlineNotification, Pagination, Row, ToastNotification, Toolbar, ToolbarBatchActions, ToolbarContent, ToolbarSearch } from "carbon-components-svelte";
    import Layout from "./shared/layout.svelte";

    import { activeProject } from "./shared/stores";
    import { objectsClient } from "./shared/pm4net-client-config";
    import type { DataTableRowId } from "carbon-components-svelte/types/DataTable/DataTable.svelte";
    import { Clean } from "carbon-icons-svelte";

    let pagination = { pageSize: 10, page: 1, filteredRowIds: <number[]>[] }
    let active = false;
    let selectedRowIds : DataTableRowId[];

    // Load object types and attributes of active project from API
    let objects = objectsClient.getObjectTypeInfos($activeProject);

    // The state of the error notification that is shown when an API error occurs
    let errorNotification = {
        show: false,
        message: ""
    }
</script>

<Layout>
    {#if $activeProject}
        {#await objects}
            <DataTableSkeleton />
        {:then objects}
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
                sortKey="occurrences"
                sortDirection="descending"
                title="Object types"
                description="Overview of all object types in the log and how often they occur."
                headers={[
                    { key: "id", value: "Name" },
                    { key: "occurrences", value: "Occurrences" }
                ]}
                rows={objects}
                pageSize={pagination.pageSize}
                page={pagination.page}
                selectable
                batchSelection={active}
                bind:selectedRowIds
                expandable>
            
                <Toolbar>
                    <ToolbarBatchActions 
                        bind:active
                        on:cancel={(e) => {
                            e.preventDefault();
                            active = false;
                        }}>
                        <Button icon={Clean} on:click={() => {
                            console.log("TODO");
                            selectedRowIds = [];
                            active = false;
                        }}>Convert to attributes</Button>
                    </ToolbarBatchActions>
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
                        <Button on:click={() => (active = true)}>Batch select</Button>
                    </ToolbarContent>
                </Toolbar>

                <svelte:fragment slot="expanded-row" let:row>
                    <pre>{JSON.stringify(row, null, 2)}</pre> <!-- TODO -->
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
            subtitle="Your object types and attributes will then be shown here."
            lowContrast />

        <DataTableSkeleton />
    {/if}
</Layout>
