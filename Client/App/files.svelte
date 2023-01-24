<script lang="ts">
    import Layout from "./shared/layout.svelte";
    import { DataTableSkeleton, InlineNotification } from "carbon-components-svelte";

    import { activeProject } from "./shared/stores";
</script>

<Layout>
    {#if $activeProject}
        <DataTableSkeleton />
    {:else}
        <InlineNotification
            title="Please activate a project before visiting this page."
            subtitle="Your log files will then be shown here."
            lowContrast />

        <DataTableSkeleton />
    {/if}
</Layout>
