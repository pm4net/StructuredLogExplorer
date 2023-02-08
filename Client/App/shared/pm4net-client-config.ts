import { ProjectClient, FileClient, MapClient } from "./pm4net-client";

const baseUrl = "";

export const projectClient = new ProjectClient(baseUrl);
export const filesClient = new FileClient(baseUrl);
export const mapClient = new MapClient(baseUrl);