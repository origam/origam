import { action } from "mobx";

export class LookupCacheDependencies {
  // Mapping semantics: {LookupCacheKey: [LookupId]}
  items = new Map<string, Set<string>>();

  heldLookupIds = new Set<string>();

  @action.bound putValues(values: { [key: string]: string[] }) {
    for (let [lookupId, newDeps] of Object.entries(values)) {
      for (let newDep of newDeps) {
        if (!this.items.has(newDep)) {
          this.items.set(newDep, new Set());
        }
        const lookupRecord = this.items.get(newDep)!;
        lookupRecord.add(lookupId);
      }
      this.heldLookupIds.add(lookupId);
    }
  }

  getUnhandledLookupIds(inputLookupIds: Set<string>) {
    return new Set(Array.from(inputLookupIds).filter((id) => !this.heldLookupIds.has(id)));
  }

  getDependencyLookupIdsByCacheKeys(cacheKeys: string[]) {
    const collectedIds = new Set<string>();
    for (let key of cacheKeys) {
      const ids = this.items.get(key) || [];
      for (let id of ids) collectedIds.add(id);
    }
    return collectedIds;
  }
}
