import { SwaggerException } from "./client";

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
    } else {
        return "Unknown error";
    }
}