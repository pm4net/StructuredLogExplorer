import { SwaggerException } from "./pm4net-client";

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