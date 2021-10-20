/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

import { action } from "mobx";
import { TypeSymbol } from "dic/Container";
import { Clock } from "./Clock";
import { LookupLabelsCleanerReloader, } from "./LookupCleanerLoader";

export class LookupCacheMulti {
  constructor(
    private clock: Clock,
    private cleanerReloader: (lookupId: string) => LookupLabelsCleanerReloader
  ) {
  }

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
