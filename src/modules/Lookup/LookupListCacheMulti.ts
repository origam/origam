import { TypeSymbol } from "dic/Container";
import { action } from "mobx";
import { IClock } from "./Clock";
import { IGetLookupLabelsCleanerReloader } from "./LookupCleanerLoader";

export class LookupListCacheMulti {
  constructor(private clock = IClock()) {}

  lists = new Map<string, any[][]>();
  recordBirthdate = new Map<string, number>();

  disposers: any[] = [];

  startup() {
    this.disposers.push(this.clock.setInterval(this.handleOutdatingTimerTick, 60 * 1000));
  }

  teardown() {
    for (let d of this.disposers) d();
  }

  @action.bound
  handleOutdatingTimerTick() {
    const now = this.clock.getTimeMs();
    const idsToClean: string[] = [];
    for (let lookupId of this.lists.keys()) {
      if (now - this.recordBirthdate.get(lookupId)! >= 10 * 60 * 1000) {
        idsToClean.push(lookupId);
      }
    }
    for (let id of idsToClean) {
      this.lists.delete(id);
      this.recordBirthdate.delete(id);
    }
  }

  getLookupList(lookupId: string) {
    return this.lists.get(lookupId);
  }

  hasLookupList(lookupId: string) {
    return this.lists.has(lookupId);
  }

  setLookupList(lookupId: string, rows: any[][]) {
    this.lists.set(lookupId, rows);
    this.recordBirthdate.set(lookupId, this.clock.getTimeMs());
  }
}
export const ILookupListCacheMulti = TypeSymbol<LookupListCacheMulti>("ILookupListCacheMulti");
