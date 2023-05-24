import type { OcelValue } from "../shared/pm4net-client";

export function getStringValue(ocelVal: OcelValue) : string {
    // @ts-ignore
    return ocelVal.value ?? "";

    /*if (ocelVal instanceof OcelString) {
        return ocelVal.value;
    } else {
        return undefined;
    }*/
}