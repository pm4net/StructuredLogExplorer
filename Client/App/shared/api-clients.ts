import { FileClient, ProjectClient } from "./client";

const baseUrl = "https://localhost:5001"; // TODO: Store this in an environment-dependent config file

export const projectClient = new ProjectClient(baseUrl);
export const filesClient = new FileClient(baseUrl);