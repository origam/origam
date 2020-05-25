import { getIdent } from "utils/common";

export function find(xmlObj: any, pred: (node: any) => boolean) {
  function recursive(n: any) {
    if (xmlObj !== n && pred(n)) {
      result.push(n);
    }
    if (n.elements) {
      for (let chn of n.elements) {
        recursive(chn);
      }
    }
  }

  const result: any[] = [];
  recursive(xmlObj);
  return result;
}

export function findStopping(xmlObj: any, pred: (node: any) => boolean) {
  function recursive(n: any) {
    if (xmlObj !== n && pred(n)) {
      result.push(n);
    } else if (n.elements) {
      for (let chn of n.elements) {
        recursive(chn);
      }
    }
  }

  const result: any[] = [];
  recursive(xmlObj);
  return result;
}

export function assignIIds(xmlTree: any) {
  function recursive(node: any) {
    node.$iid = getIdent();
    if (node.elements) {
      for (let e of node.elements) recursive(e);
    }
  }
  recursive(xmlTree);
}
