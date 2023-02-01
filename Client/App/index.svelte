<script lang="ts">
    import Layout from "./shared/layout.svelte";
    import { FolderAdd, TrashCan, ChooseItem, ResetAlt } from "carbon-icons-svelte";
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

    import { get } from "svelte/store";
    import isValidFilename from "valid-filename";

    import { getErrorMessage } from "./shared/helpers";
    import { projectClient } from "./shared/api-clients";
    import { getFromJson } from "./shared/config";
    import { activeProject } from "./shared/stores";

    let pagination = { pageSize: 10, page: 1, filteredRowIds: <number[]>[] }

    // The list of available projects, retrieved from a hidden input with JSON data
    let projects = getFromJson<{
        id: string;
        name: string;
        logDirectory: string;
        noOfEvents: number;
        noOfObjects: number;
        active: boolean;
    }[]>("projects").map((val, _) => {
        val.id = val.name; // Each row has to have an ID
        val.active = val.name === get(activeProject); // Set active state based on value in local storage
        return val;
    });

    // Remove active project if it isn't in list of projects anymore (e.g. file got deleted)
    if (projects.find(p => p.name === get(activeProject)) === undefined) {
        activateProject(null);
    }

    // The state of the modal to create a new project
    let createModal = {
        open: false,
        project: { value: "", invalid: false },
        logDirectory: { value: "", invalid: false },
        errorNotification: {
            show: false,
            message: ""
        }
    }

    // Close the modal and reset all fields
    function closeAndResetCreateModal() {
        createModal.open = false;
            createModal.project.value = "";
            createModal.project.invalid = false;
            createModal.logDirectory.value = "";
            createModal.logDirectory.invalid = false;
            createModal.errorNotification.show = false;
            createModal.errorNotification.message = "";
    }

    // Update activated route to disable button, update previously active row to enable button, and save in local storage
    function activateProject(name: string | null) {
        let prevActiveIdx = projects.findIndex(p => p.name === get(activeProject));
        if (prevActiveIdx !== -1) {
            projects[prevActiveIdx].active = false;
        }
        
        if (typeof name === "string") {
            let newActiveIdx = projects.findIndex(p => p.name === name);
            projects[newActiveIdx].active = true;
        }

        activeProject.set(name);
    }

    /// Create a new project with the values entered in the modal
    async function createProject() {
        createModal.errorNotification.show = false;
        createModal.errorNotification.message = "";

        try {
            validateProjectName();
            validateLogDirectory();

            if (!createModal.project.invalid && !createModal.logDirectory.invalid) {
                await projectClient.create(createModal.project.value, createModal.logDirectory.value);

                // Add new project to the list
                projects = [...projects, {
                    id: createModal.project.value,
                    name: createModal.project.value,
                    logDirectory: createModal.logDirectory.value,
                    active: false,
                    noOfEvents: 0,
                    noOfObjects: 0
                }];
                
                closeAndResetCreateModal();
            }
        } catch (e: unknown) {
            createModal.errorNotification.show = true;
            createModal.errorNotification.message = getErrorMessage(e);
        }
    }

    // Delete a project from disk
    async function deleteProject(name: string) {
        try {
            await projectClient.delete(name);
            projects = projects.filter(p => p.name !== name);

            if (get(activeProject) === name) {
                activateProject(null);
            }
        } catch (e: unknown) {
            // TODO: Show modal with error message
            console.error(e);
        }
    }

    // Validate whether the entered file name looks like a correct filename (without extension), and that it doesn't already exist
    function validateProjectName() {
        createModal.project.invalid =
            createModal.project.value === "" ||
            !isValidFilename(createModal.project.value) ||
            projects.some(p => p.name == createModal.project.value);
    }

    // Validate that the log directory is not empty
    function validateLogDirectory() {
        createModal.logDirectory.invalid = createModal.logDirectory.value === "";
    }
</script>

<Layout>
    
    <DataTable 
        sortable
        sortKey="name"
        title="Projects" 
        description="Each project points to a specific directory with log files."
        headers={[
            { key: "name", value: "Name" },
            { key: "logDirectory", value: "Log Directory" },
            { key: "noOfEvents", value: "Events" },
            { key: "noOfObjects", value: "Objects" },
            { key: "overflow", empty: true }
        ]}
        bind:rows={projects}
        pageSize={pagination.pageSize}
        page={pagination.page}>

        <Toolbar>
            <ToolbarContent>
                <ToolbarSearch persistent shouldFilterRows bind:filteredRowIds={pagination.filteredRowIds} />
                <Button icon={FolderAdd} on:click={() => createModal.open = true}>Create</Button>
            </ToolbarContent>
        </Toolbar>

        <svelte:fragment slot="cell" let:row let:cell>
            {#if cell.key === "overflow"}
                {#if row.active}
                    <Button
                        kind="secondary"
                        icon={ResetAlt}
                        on:click={() => activateProject(null)}>
                        Deactivate
                    </Button>
                {:else}
                    <Button
                        icon={ChooseItem}
                        on:click={() => activateProject(row.id)}>
                        Activate
                    </Button>
                {/if}
                
                <Button
                    on:click={() => deleteProject(row.id)} 
                    kind="ghost" 
                    iconDescription="Delete project" 
                    icon={TrashCan}>
                </Button>
            {:else}{cell.value}{/if}
        </svelte:fragment>

    </DataTable>

    <Pagination
        bind:pageSize={pagination.pageSize}
        bind:page={pagination.page}
        totalItems={pagination.filteredRowIds.length}
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

        {#if createModal.errorNotification.show}
            <InlineNotification
                subtitle={createModal.errorNotification.message}
                lowContrast
                on:close={() => createModal.errorNotification.show = false}
            >
                <strong slot="title">Error:</strong>
            </InlineNotification>
        {/if}

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
