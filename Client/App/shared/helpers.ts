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
export function getLines(canvas: any, text: string, maxWidth: number, maxHeight: number, font?: string) {
    const context = canvas.getContext("2d")!;
    if (font) {
        context.font = font;
    }

    var words = text.split(" ");
    var lines = [];
    var currentLine = words[0];
    var currentHeight = 0;

    for (var i = 1; i < words.length; i++) {
        var word = words[i];
        var measurement = context.measureText(currentLine + " " + word);
        var width = measurement.width;
        var height = measurement.actualBoundingBoxAscent + measurement.actualBoundingBoxDescent; // https://stackoverflow.com/a/45789011/2102106

        if (currentHeight + height > maxHeight) {
            break;
        }

        if (width < maxWidth) {
            currentLine += " " + word;
        } else {
            lines.push(currentLine);
            currentLine = word;
            currentHeight += height;
        }
    }
    lines.push(currentLine);
    return lines;
}

export function createOcDfgOptionsFromStore() {
    let settings = get(mapSettings)[get(activeProject) ?? ""];
    return new OcDfgOptions({
        minimumEvents: settings?.dfg.minEvents,
        minimumOccurrence: settings?.dfg.minOccurrences,
        minimumSuccessions: settings?.dfg.minSuccessions,
        includedTypes: settings?.objectTypes,
        includedLogLevels: settings.logLevels,
        includedNamespaces: settings?.namespaces,
        dateFrom: `${settings?.dfg.dateFrom} ${settings?.dfg.timeFrom}`,
        dateTo: `${settings?.dfg.dateTo} ${settings?.dfg.timeTo}`,
        keepCases: settings?.dfg.keepCases
    });
}