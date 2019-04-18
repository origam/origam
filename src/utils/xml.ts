import { IViewType } from "../DataView/types/IViewType";
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

export function parseNumber(val: string) {
  return parseInt(val, 10);
}

export function parseBoolean(val: string) {
  return val === "true";
}

export function parseViewType(vt: string) {
  switch (vt) {
    case "0":
      return IViewType.Form;
    case "1":
      return IViewType.Table;
    default:
      return undefined;
  }
}
