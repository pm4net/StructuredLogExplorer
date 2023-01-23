import { baseUrl } from "./config";
import { ProjectClient } from "./client";

export const projectClient = new ProjectClient(baseUrl);