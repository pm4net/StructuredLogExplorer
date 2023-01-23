<script lang="ts">
    import Layout from "./shared/layout.svelte";
    import {
        Button,
        DataTable,
        Modal,
        Pagination,
        Row,
        TextInput,
        Toolbar,
        ToolbarContent,
        ToolbarMenu,
        ToolbarMenuItem,
        ToolbarSearch
    } from "carbon-components-svelte";
    import FolderAdd from "carbon-icons-svelte/lib/FolderAdd.svelte";

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

    let pagination = {pageSize: 10, page: 1}

    let createModalOpen = false;
    let createModalProjectName = "";
    let createModalLogDirectory = "";

    function createProject() : void {
        
    }
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

        <Toolbar>
            <ToolbarContent>
                <ToolbarSearch persistent shouldFilterRows />
                <Button icon={FolderAdd} on:click={() => createModalOpen = true}>Create</Button>
            </ToolbarContent>
        </Toolbar>

    </DataTable>

    <Pagination
        bind:pageSize={pagination.pageSize}
        bind:page={pagination.page}
        totalItems={projects.length}
        pageSizeInputDisabled
    />

    <Modal
        bind:open={createModalOpen}
        modalHeading="Create new project"
        primaryButtonText="Create"
        secondaryButtonText="Cancel"
        selectorPrimaryFocus="#project-name"
        on:click:button--primary={createProject}
        on:click:button--secondary={() => createModalOpen = false}
        on:open
        on:close
        on:submit
    >
        <!-- TODO: https://github.com/sindresorhus/valid-filename -->
        <TextInput
            bind:value={createModalProjectName}
            id="project-name"
            labelText="Project name"
            placeholder="Enter your project's name..."
            invalidText="Project name must be a valid file name"
            invalid={createModalProjectName === ""}
        />

        <br />

        <TextInput
            bind:value={createModalLogDirectory}
            id="log-dir"
            label="Log directory"
            placeholder="Enter the directory where your log files are stored..."
            invalidText="Log directory must be a valid directory"
            invalid={createModalLogDirectory === "test"}
        />
    </Modal>

</Layout>
