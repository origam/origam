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
