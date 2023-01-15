import { getFromJson } from "./config";

const urls = getFromJson<{
    indexUrl: string;
    aboutUrl: string;
    spaUrl: string;
}>("urls");

export default urls;