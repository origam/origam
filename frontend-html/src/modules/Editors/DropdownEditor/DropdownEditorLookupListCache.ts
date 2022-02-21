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
import { LookupListCacheMulti } from "modules/Lookup/LookupListCacheMulti";
import { DropdownEditorSetup } from "modules/Editors/DropdownEditor/DropdownEditorSetup";

export class DropdownEditorLookupListCache {
  constructor(private setup: () => DropdownEditorSetup, private cache: LookupListCacheMulti) {
  }

  get lookupId() {
    return this.setup().lookupId;
  }

  setCachedListRows(rows: any[][]) {
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
