<script lang="ts">
    import { Button, Slider } from "carbon-components-svelte";
    import { PauseFilled, PlayFilledAlt } from "carbon-icons-svelte";
    import { createEventDispatcher } from "svelte";

    export let min : number;
    export let max : number;

    const dispatch = createEventDispatcher<{sliderChange: number}>();

    let value : number;
    let playing = false;
    let interval : NodeJS.Timer;

    function startOrPauseReplay() {
        if (!playing) {
            playing = true;

            // Start from the beginning when clicking play but is already at the end
            if (value == max) {
                value = min;
            }

            // Dispatch first event without waiting so as not to skip first value
            dispatch("sliderChange", value);
            interval = setInterval(() => {
                value += 1;
                if (value == max) {
                    playing = false;
                    clearInterval(interval);
                }
            }, 2000);
        } else {
            playing = false;
            clearInterval(interval);
        }
    }

    export function sliderChange() {
        dispatch("sliderChange", value);
    }
</script>

<div class="replay-control">
    <div class="play-button">
        <Button
            kind="ghost"
            iconDescription={playing ? "Pause" : "Play"}
            icon={playing ? PauseFilled : PlayFilledAlt}
            tooltipPosition="top"
            on:click={startOrPauseReplay}>
        </Button>
    </div>
    <div class="replay-slider">
        <Slider fullWidth hideTextInput {min} {max} bind:value on:change={sliderChange}></Slider>
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