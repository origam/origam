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

import { TypeSymbol } from "dic/Container";
import { action } from "mobx";
import { IClock } from "./Clock";

export class LookupListCacheMulti {
  constructor(private clock = IClock()) {
  }

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
      this.deleteLookup(id);
    }
  }

  @action.bound
  deleteLookup(id: string) {
    this.lists.delete(id);
    this.recordBirthdate.delete(id);
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
