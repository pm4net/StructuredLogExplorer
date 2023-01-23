export function getActiveProject() : string | null {
    return localStorage.getItem("activeProject");
}

export function setActiveProject(name: string) : void {
    localStorage.setItem("activeProject", name);
}