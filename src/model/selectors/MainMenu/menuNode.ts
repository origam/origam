import {T} from "utils/translation";

export function getAllParents(node: any) {
  const parents: any[] = [];
  let cn = node;
  while (cn !== undefined) {
    parents.push(cn);
    cn = cn.parent;
  }
  parents.reverse();
  return parents.slice(2); // Strip out root and Menu node
}

export function getPath(node: any) {
  const nodeLabels = getAllParents(node).map(node => node.attributes.label);
  nodeLabels.unshift(T("Menu", "menu"));
  return nodeLabels.join("/")
}
