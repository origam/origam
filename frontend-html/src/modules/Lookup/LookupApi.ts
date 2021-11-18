import { delay } from "./common";
import { TypeSymbol } from "dic/Container";
import { IOrigamAPI } from "model/entities/OrigamAPI";
import { IApi } from "model/entities/types/IApi";

export class LookupApi {
  constructor(private api: () => IApi) {}

  async getLookupLabels(request: Map<string, Map<any, any>>) {
    const requestRaw: any[] = [];
    for (let [lookupId, v1] of request) {
      requestRaw.push({
        LookupId: lookupId,
        MenuId: undefined,
        LabelIds: Array.from(v1.keys()),
      });
    }

    const resultRaw: { [k: string]: any } = await this.api().getLookupLabelsEx(requestRaw);
    const result = new Map<string, Map<any, any>>();
    for (let [lookupId, lookupResolved] of Object.entries(resultRaw)) {
      if (!result.has(lookupId)) {
        result.set(lookupId, new Map());
      }
      const lookupMap = result.get(lookupId)!;
      for (let [labelId, labelValue] of Object.entries(lookupResolved)) {
        lookupMap.set(labelId, labelValue);
      }
    }
    return result;
  }
}
export const ILookupApi = TypeSymbol<LookupApi>("ILookupApi");
