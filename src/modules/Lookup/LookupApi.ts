import { delay } from "./common";
import { TypeSymbol } from "dic/Container";
import { IOrigamAPI } from "model/entities/OrigamAPI";

export class LookupApi {
  constructor(private api = IOrigamAPI()) {}

  async getLookupLabels(request: Map<string, Map<any, any>>) {
    /*await delay(1000);
    let somenum = 19;
    const result = new Map<string, Map<any, any>>();
    for (let [l1k, l1v] of request.entries()) {
      result.set(l1k, new Map());
      for (let [l2k, l2v] of l1v) {
        result.get(l1k)!.set(l2k, somenum);
        somenum = somenum + 97;
      }
    }*/
    
    const requestRaw: any[] = [];
    for(let [lookupId, v1] of request) {
      requestRaw.push({
        LookupId: lookupId,
        MenuId: undefined,
        LabelIds: Array.from(v1.keys())
      })
    }

    const resultRaw: {[k: string]: any} = await this.api.getLookupLabelsEx(requestRaw)
    const result = new Map<string, Map<any, any>>();
    for(let [lookupId, lookupResolved] of Object.entries(resultRaw)) {
      if(!result.has(lookupId)) {
        result.set(lookupId, new Map());
      }
      const lookupMap = result.get(lookupId)!;
      for(let [labelId, labelValue] of Object.entries(lookupResolved)) {
        lookupMap.set(labelId, labelValue);
      }
  
    }
    return result;
  }

  async getLookupList(lookupId: string) {
    await delay(1000);
    return []
  }
}
export const ILookupApi = TypeSymbol<LookupApi>("ILookupApi");
