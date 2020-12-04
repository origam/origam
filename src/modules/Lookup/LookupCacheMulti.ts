import { action } from "mobx";
import { TypeSymbol, Func } from "dic/Container";
import { IClock, Clock } from "./Clock";
import { FORMERR } from "dns";
import {
  ILookupLabelsCleanerReloader,
  IGetLookupLabelsCleanerReloader,
  LookupLabelsCleanerReloader,
} from "./LookupCleanerLoader";

export class LookupCacheMulti {
  constructor(
    private clock: Clock,
    private cleanerReloader: (lookupId: string) => LookupLabelsCleanerReloader
  ) {}

  labels = new Map<string, Map<any, any>>();
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
    for (let lookupId of this.labels.keys()) {
      if (now - this.recordBirthdate.get(lookupId)! >= 10 * 60 * 1000) {
        idsToClean.push(lookupId);
      }
    }
    for (let id of idsToClean) {
      this.cleanerReloader(id).reloadLookupLabels();
    }
  }

  getLookupLabels(lookupId: string) {
    return this.labels.get(lookupId);
  }

  addLookupLabels(lookupId: string, labels: Map<any, any>) {
    if (!this.labels.has(lookupId)) {
      this.labels.set(lookupId, new Map());
      this.recordBirthdate.set(lookupId, this.clock.getTimeMs());
    }
    const indivLabels = this.labels.get(lookupId)!;
    for (let [k, v] of labels.entries()) {
      indivLabels.set(k, v);
    }
  }

  @action.bound
  clean(lookupId: string) {
    this.labels.delete(lookupId);
    this.recordBirthdate.delete(lookupId);
  }
}
export const ILookupCacheMulti = TypeSymbol<LookupCacheMulti>("ILookupCacheMulti");
