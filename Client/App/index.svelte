<script lang="ts">
    import Layout from "./shared/layout.svelte";
    import { 
        Select, 
        SelectItem, 
    
        DataTable,
        Toolbar,
        ToolbarContent,
        ToolbarSearch,
        ToolbarMenu,
        ToolbarMenuItem,
        Pagination,

        Button } from "carbon-components-svelte";

    import { getFromJson } from "./shared/config";

    let projects = getFromJson<{
        id: string;
        name: string;
        logDirectory: string;
        isLoaded: boolean;
    }[]>("projects");

    let pagination = {pageSize: 10, page: 0}
</script>

<Layout>
    <DataTable sortable title="Projects" description="Each project points to a specific directory with log files."
        headers={[
            { key: "name", value: "Name" },
            { key: "logDirectory", value: "Log Directory" },
            { key: "isLoaded", value: "Is Loaded" }
        ]}
        pageSize={pagination.pageSize}
        page={pagination.page}
        rows={projects}>
    </DataTable>

    <Pagination
        bind:pageSize={pagination.pageSize}
        bind:page={pagination.page}
        totalItems={projects.length}
        pageSizeInputDisabled
    />

</Layout>
