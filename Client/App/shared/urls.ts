import { getFromJson } from "./config";

const urls = getFromJson<{
    homeUrl: string;
    filesUrl: string;
    mapUrl: string;
    statisticsUrl: string;
    casesUrl: string;
}>("urls");

export default urls;