import uniqolor from "uniqolor";

export function getColor(text: string) {
    return uniqolor(text).color;
}