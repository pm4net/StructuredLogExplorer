import { getFromJson } from "./config";

const urls = getFromJson<{
    homeUrl: string;
    filesUrl: string;
    objectsUrl: string;
    mapUrl: string;
    statisticsUrl: string;
    casesUrl: string;
}>("urls");

export default urls;