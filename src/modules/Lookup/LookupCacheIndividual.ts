import { action } from "mobx";
import { LookupCacheMulti } from "./LookupCacheMulti";
import { TypeSymbol } from "dic/Container";

export class LookupCacheIndividual {
  constructor(private lookupId: string, private cache: LookupCacheMulti) {}

  getLookupLabels() {
    return this.cache.getLookupLabels(this.lookupId) || new Map();
  }

  addLookupLabels(labels: Map<any, any>) {
    this.cache.addLookupLabels(this.lookupId, labels);
  }

  @action.bound
  clean() {
    this.cache.clean(this.lookupId);
  }
}
export const ILookupCacheIndividual = TypeSymbol<LookupCacheIndividual>("ILookupCacheIndividual");
