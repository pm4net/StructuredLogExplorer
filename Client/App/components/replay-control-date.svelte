<script lang="ts">
    import { Button, Slider } from "carbon-components-svelte";
    import { Play } from "carbon-icons-svelte";

    export let min : Date;
    export let max : Date;

    let minVal = 0;
    let maxVal = Math.ceil(getEpochTimeInSeconds(max) - getEpochTimeInSeconds(min));
    let step = Math.ceil(maxVal / 100);

    function getEpochTimeInSeconds(date: Date) {
        return date.getTime() / 1000;
    }
</script>

<div class="replay-control">
    <div class="play-button">
        <Button
            kind="ghost"
            iconDescription="Play" 
            icon={Play}
            tooltipPosition="top"
            on:click={() => {}}>
        </Button>
    </div>
    <div class="replay-slider">
        <Slider fullWidth hideTextInput min={minVal} max={maxVal} {step} minLabel={min.toLocaleString()} maxLabel={max.toLocaleString()}></Slider>
    </div>
</div>

<style lang="scss">
    .replay-control {
        position: absolute;
        z-index: 1;
        bottom: 1rem;
        left: 1rem;
        width: calc(100% - 2rem);
        height: 3rem;
    }

    .play-button {
        position: absolute;
    }

    .replay-slider {
        position: absolute;
        width: calc(100% - 4rem);
        left: 4rem;
    }
</style>