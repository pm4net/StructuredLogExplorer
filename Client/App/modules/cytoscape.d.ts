declare module "cytoscape-node-html-label";

// https://github.com/kaluginserg/cytoscape-node-html-label/issues/77
declare namespace cytoscape {
    interface Core {
      nodeHtmlLabel(options: CytoscapeNodeHtmlParams[]): void;
    }
  }