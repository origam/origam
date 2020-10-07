import { TypeSymbol } from "dic/Container";
import { LookupListCacheMulti } from "modules/Lookup/LookupListCacheMulti";
import { DropdownEditorSetup } from "./DropdownEditor";

export class DropdownEditorLookupListCache {
  constructor(private setup: () => DropdownEditorSetup, private cache: LookupListCacheMulti) {}

  get lookupId() {
    return this.setup().lookupId;
  }

  setCachedListRows(rows: any[][]) {
    console.log("Will save to cache:", rows);
    this.cache.setLookupList(this.lookupId, rows);
  }

  getCachedListRows(): any[][] {
    return this.cache.getLookupList(this.lookupId)!;
  }

  hasCachedListRows(): boolean {
    return this.cache.hasLookupList(this.lookupId);
  }

  clean() {
    return this.cache.deleteLookup(this.lookupId);
  }
}
export const IDropdownEditorLookupListCache = TypeSymbol<DropdownEditorLookupListCache>(
  "IDropdownEditorLookupListCache"
);
