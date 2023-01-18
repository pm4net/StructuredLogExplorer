<script lang="ts">
    import { InlineNotification } from "carbon-components-svelte";
    import Layout from "./shared/layout.svelte";
</script>

<Layout>
    <InlineNotification
        title="404 Not Found"
        subtitle="This page doesn't seem to exist."
        kind="error"
        hideCloseButton
        lowContrast
    />
</Layout>
