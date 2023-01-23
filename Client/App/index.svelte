<script lang="ts">
    import Layout from "./shared/layout.svelte";
    import {
        DataTable,
        Pagination
    } from "carbon-components-svelte";

    import { getFromJson } from "./shared/config";

    let projects = getFromJson<{
        id: string;
        name: string;
        logDirectory: string;
        noOfEvents: number;
        noOfObjects: number;
    }[]>("projects").map((val, _) => {
        val.id = val.name;
        return val;
    });

    console.log(projects);

    let pagination = {pageSize: 10, page: 1}
</script>

<Layout>
    <DataTable sortable title="Projects" description="Each project points to a specific directory with log files."
        headers={[
            { key: "name", value: "Name" },
            { key: "logDirectory", value: "Log Directory" },
            { key: "noOfEvents", value: "Events" },
            { key: "noOfObjects", value: "Objects" },
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
