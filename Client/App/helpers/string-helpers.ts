export function placeAroundMatches(text: string, match1: string, match2: string, before: string, after: string) {
    text = text.replaceAll(match1, before + match1);
    text = text.replaceAll(match2, match2 + after);
    return text;
}