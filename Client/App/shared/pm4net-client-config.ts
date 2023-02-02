import { FileClient, ProjectClient } from "./pm4net-client";

const baseUrl = "";

export const projectClient = new ProjectClient(baseUrl);
export const filesClient = new FileClient(baseUrl);