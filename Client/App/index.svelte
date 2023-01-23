<script lang="ts">
    import Layout from "./shared/layout.svelte";
    import {
        Button,
        DataTable,
        InlineNotification,
        Modal,
        Pagination,
        TextInput,
        Toolbar,
        ToolbarContent,
        ToolbarSearch
    } from "carbon-components-svelte";
    import FolderAdd from "carbon-icons-svelte/lib/FolderAdd.svelte";

    import { getErrorMessage } from "./shared/helpers";
    import { projectClient } from "./shared/clients";
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

    /* --- Modal logic --- */

    let createModal = {
        open: false,
        project: { value: "", invalid: false },
        logDirectory: { value: "", invalid: false }
    }

    let errorNotification = {
        show: false,
        message: ""
    }

    function validateProjectName() {
        createModal.project.invalid =
            createModal.project.value === "" ||
            projects.some(p => p.name == createModal.project.value);
    }

    function validateLogDirectory() {
        createModal.logDirectory.invalid = 
            createModal.logDirectory.value === "";
    }

    async function createProject() {
        errorNotification.show = false;
        errorNotification.message = "";

        try {
            validateProjectName();
            validateLogDirectory();

            if (!createModal.project.invalid && !createModal.logDirectory.invalid) {
                await projectClient.create(createModal.project.value, createModal.logDirectory.value);
                createModal.open = false;
                location.reload();
            }
        } catch (e: unknown) {
            errorNotification.show = true;
            errorNotification.message = getErrorMessage(e);
        }
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
                <Button icon={FolderAdd} on:click={() => createModal.open = true}>Create</Button>
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
        bind:open={createModal.open}
        modalHeading="Create new project"
        primaryButtonText="Create"
        secondaryButtonText="Cancel"
        selectorPrimaryFocus="#project-name"
        primaryButtonDisabled={createModal.project.invalid || createModal.logDirectory.invalid}
        preventCloseOnClickOutside
        on:click:button--primary={createProject}
        on:click:button--secondary={() => createModal.open = false}
        on:open
        on:close
        on:submit
    >

        {#if errorNotification.show}
            <InlineNotification
                title="Error:"
                subtitle={errorNotification.message}
                lowContrast
                on:close={() => errorNotification.show = false}
            />
        {/if}

        <!-- TODO: https://github.com/sindresorhus/valid-filename -->
        <TextInput
            bind:value={createModal.project.value}
            on:input={validateProjectName}
            id="project-name"
            labelText="Project name"
            placeholder="Enter your project's name..."
            invalidText="Must be a valid file name and can not be the same as an existing project"
            invalid={createModal.project.invalid}
        />

        <br />

        <TextInput
            bind:value={createModal.logDirectory.value}
            on:input={validateLogDirectory}
            id="log-dir"
            labelText="Log directory"
            placeholder="Enter the directory where your log files are stored..."
            invalidText="Must be a valid directory"
            invalid={createModal.logDirectory.invalid}
        />
    </Modal>

</Layout>
