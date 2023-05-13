<script lang="ts">
    import { Button, CodeSnippet, DataTable, DataTableSkeleton, ExpandableTile, InlineNotification, Pagination, Tile, ToastNotification, Toolbar, ToolbarBatchActions, ToolbarContent, ToolbarSearch } from "carbon-components-svelte";
    import Layout from "./shared/layout.svelte";
    import { activeProject } from "./shared/stores";
    import { objectsClient } from "./shared/pm4net-client-config";
    import type { DataTableRowId } from "carbon-components-svelte/types/DataTable/DataTable.svelte";
    import { Clean } from "carbon-icons-svelte";
    import { getErrorMessage } from "./shared/helpers";
    
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

    async function convertObjectsToAttributes(objectTypes: string[]) {
        try {
            await objectsClient.convertObjectsToAttributes($activeProject, objectTypes);
            selectedRowIds = [];
            active = false;
        } catch (e: unknown) {
            errorNotification.show = true;
            errorNotification.message = getErrorMessage(e);
        } finally {
            objects = objectsClient.getObjectTypeInfos($activeProject);
        }
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
                sortKey="uniqueInstances"
                sortDirection="descending"
                title="Object types"
                description="Overview of all object types in the log and how often they occur."
                headers={[
                    { key: "id", value: "Name" },
                    { key: "uniqueInstances", value: "Unique instances" },
                    { key: "referencingEvents", value: "Referencing events" }
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
                        <Button icon={Clean} on:click={() => convertObjectsToAttributes(selectedRowIds)}>Convert to attributes</Button>
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
                    {#each row.objectOccurrences as occurrence}
                        {#if occurrence.codeSnippet}
                            <ExpandableTile light tileExpandedLabel="Hide code snippet" tileCollapsedLabel="Show code snippet">
                                <div slot="above">
                                    <p><strong>Activity: </strong>{occurrence.activity}</p>
                                    {#if occurrence.namespace}
                                        <p><strong>Namespace: </strong>{occurrence.namespace}</p>
                                    {/if}
                                    {#if occurrence.sourceFile}
                                        <p><strong>Source File: </strong>{occurrence.sourceFile}</p>
                                    {/if}
                                    {#if occurrence.lineNumber}
                                        <p><strong>Line Number: </strong>{occurrence.lineNumber}</p>
                                    {/if}
                                </div>
                                <!-- Would use if here to avoid code duplication, but it is currrently not supported: https://github.com/sveltejs/svelte/issues/5604 -->
                                <div slot="below">
                                    <CodeSnippet 
                                        type="multi" 
                                        code={occurrence.codeSnippet}
                                    /> <!-- copy={(_) => window.open(`file:///${occurrence.sourceFile}`)} // TODO: Doesn't work due to security -->
                                </div>
                            </ExpandableTile>
                        {:else}
                            <Tile>
                                <p><strong>Activity: </strong>{occurrence.activity}</p>
                                {#if occurrence.namespace}
                                    <p><strong>Namespace: </strong>{occurrence.namespace}</p>
                                {/if}
                                {#if occurrence.sourceFile}
                                    <p><strong>Source File: </strong>{occurrence.sourceFile}</p>
                                {/if}
                                {#if occurrence.lineNumber}
                                    <p><strong>Line Number: </strong>{occurrence.lineNumber}</p>
                                {/if}
                            </Tile>
                        {/if}
                        
                    {/each}
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

<style lang="scss">
    :global(.bx--snippet) {
        max-width: none;
    }
</style>