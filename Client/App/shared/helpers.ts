import { get } from "svelte/store";
import { OcDfgOptions, SwaggerException } from "./pm4net-client";
import { activeProject, mapSettings } from "./stores";

export function getErrorMessage(e: unknown) : string {
    if (e instanceof SwaggerException) {
        if (e.status != 500) {
            let json = JSON.parse(e.response);
            let errors = json.errors;
            if (errors) {
                let errorMsgs : string[][] = []
                Object.keys(errors).forEach((key, _) => { errorMsgs.push(errors[key]); });
                return errorMsgs.reduce((acc, val) => acc.concat(val), []).join(", ");
            } else {
                return e.response;
            }
        } else {
            return e.response;
        }
    } else if (e instanceof Error) {
        return e.message;
    } else {
        return "Unknown error.";
    }
}

/**
  * Uses canvas.measureText to compute and return the width and height of the given text of given font in pixels.
  * 
  * @param {String} text The text to be rendered.
  * @param {String} font The css font descriptor that text is to be rendered with (e.g. "bold 14px verdana").
  * 
  * @see https://stackoverflow.com/questions/118241/calculate-text-width-with-javascript/21015393#21015393
  */
export function getTextSize(canvas: any, text: string, font?: string) {
    const context = canvas.getContext("2d")!;
    if (font) {
        context.font = font;
    }

    const lines = text.split("\n");
    const measured = lines.map(l => context.measureText(l));
    return { 
        width: Math.max(...measured.map(m => m.width)), 
        height: measured.map(m => m.actualBoundingBoxAscent + m.actualBoundingBoxDescent).reduce((acc, height) => acc + height)
    };
}

// https://stackoverflow.com/a/16599668/2102106
export function getLines(canvas: any, text: string, maxWidth: number, font?: string) {
    const context = canvas.getContext("2d")!;
    if (font) {
        context.font = font;
    }

    var words = text.split(" ");
    var lines = [];
    var currentLine = words[0];

    for (var i = 1; i < words.length; i++) {
        var word = words[i];
        var width = context.measureText(currentLine + " " + word).width;
        if (width < maxWidth) {
            currentLine += " " + word;
        } else {
            lines.push(currentLine);
            currentLine = word;
        }
    }
    lines.push(currentLine);
    return lines;
}

export function createOcDfgOptionsFromStore() {
    return new OcDfgOptions({
        minimumEvents: get(mapSettings)[get(activeProject) ?? ""]?.dfg.minEvents,
        minimumOccurrence: get(mapSettings)[get(activeProject) ?? ""]?.dfg.minOccurrences,
        minimumSuccessions: get(mapSettings)[get(activeProject) ?? ""]?.dfg.minSuccessions,
        includedTypes: get(mapSettings)[get(activeProject) ?? ""]?.objectTypes,
        includedLogLevels: get(mapSettings)[get(activeProject) ?? ""].logLevels,
        dateFrom: get(mapSettings)[get(activeProject) ?? ""]?.dfg.dateFrom,
        dateTo: get(mapSettings)[get(activeProject) ?? ""]?.dfg.dateTo,
        keepCases: get(mapSettings)[get(activeProject) ?? ""]?.dfg.keepCases
    });
}