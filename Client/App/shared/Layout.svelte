<script lang="ts">
    import {
        Header,
        HeaderNav,
        HeaderNavItem,
        HeaderUtilities,
        HeaderActionLink,
        HeaderAction,
        HeaderPanelLinks,
        HeaderPanelDivider,
        HeaderPanelLink,

        SideNav,
        SideNavItems,
        SideNavLink,
        SkipToContent,

        Content,
        Grid,
        Row,
        Column,
        Theme
    } from "carbon-components-svelte";

    import urls from "./urls";
    import { get } from "./config";
    import { getActiveProject } from "./storage";

    let title = get<string>("title");

    let isSideNavOpen = false;
    let isMenuOpen = false;

    const links = [
        {href: urls.homeUrl, text: "Home", isSelected: title === "Home"},
        {href: urls.filesUrl, text: "Files", isSelected: title === "Files"},
        {href: urls.mapUrl, text: "Map", isSelected: title === "Map"},
        {href: urls.statisticsUrl, text: "Statistics", isSelected: title === "Statistics"},
        {href: urls.casesUrl, text: "Cases", isSelected: title === "Cases"},
    ];

    let theme: any = "g100";

    function setTheme(value: string) {
        theme = value;
        isMenuOpen = false;
    }
</script>

<Theme bind:theme persist persistKey="carbon-theme" />

<Header persistentHamburgerMenu={true} company="pm4net" platformName={getActiveProject() ?? "No project selected"} bind:isSideNavOpen>
    <svelte:fragment slot="skip-to-content">
        <SkipToContent />
    </svelte:fragment>
    <HeaderNav>
        {#each links as link}
            <HeaderNavItem href="{link.href}" text="{link.text}" isSelected="{link.isSelected}" />
        {/each}
    </HeaderNav>

    <HeaderUtilities>
        <HeaderAction bind:isOpen={isMenuOpen}>
            <HeaderPanelLinks>
                <HeaderPanelDivider>Select Theme</HeaderPanelDivider>
                <HeaderPanelLink class="{theme=="white"?"bx--menu-option--active":""}" on:click={()=>setTheme("white")}>White</HeaderPanelLink>
                <HeaderPanelLink class="{theme=="g10"?"bx--menu-option--active":""}" on:click={()=>setTheme("g10")}>Light Gray</HeaderPanelLink>
                <HeaderPanelLink class="{theme=="g80"?"bx--menu-option--active":""}" on:click={()=>setTheme("g80")}>Gray</HeaderPanelLink>
                <HeaderPanelLink class="{theme=="g90"?"bx--menu-option--active":""}" on:click={()=>setTheme("g90")}>Dark Gray</HeaderPanelLink>
                <HeaderPanelLink class="{theme=="g100"?"bx--menu-option--active":""}" on:click={()=>setTheme("g100")}>Dark</HeaderPanelLink>
            </HeaderPanelLinks>
        </HeaderAction>
    </HeaderUtilities>

    <SideNav bind:isOpen={isSideNavOpen}>
        <SideNavItems>
            {#each links as link}
                <SideNavLink href="{link.href}" text="{link.text}" isSelected="{link.isSelected}" />
            {/each}
        </SideNavItems>
    </SideNav>
</Header>


<Content class="content">
    <Grid>
        <Row>
            <Column>
                <slot></slot>
            </Column>
        </Row>
    </Grid>
</Content>

<style lang="scss">
:global(.content) {
    background-color: transparent;
}
:global(.activeThemeLink) {
    background-color: gray;
}
</style>
