declare module "cytoscape-node-html-label";
declare module "cytoscape-view-utilities";
declare module "cytoscape-svg";

// https://github.com/kaluginserg/cytoscape-node-html-label/issues/77
declare namespace cytoscape {
    interface Core {
      nodeHtmlLabel(options: CytoscapeNodeHtmlParams[]): void;
      viewUtilities(options?: any): any;
    }
  }