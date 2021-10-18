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

import { IFilterSetting } from "../../../../../../model/entities/types/IFilterSetting";

import { observable } from "mobx";

export const EDITOR_DALEY_MS = 500;

export class FilterSetting implements IFilterSetting {
  @observable type: string;
  @observable val1?: any;
  @observable val2?: any;
  isComplete: boolean;
  lookupId: string | undefined;

  get filterValue1() {
    return this.val1;
  }

  get filterValue2() {
    return this.type === "between" || this.type === "nbetween"
      ? this.val2
      : undefined;
  }

  get val1ServerForm() {
    return this.val1;
  }

  get val2ServerForm() {
    return this.val2;
  }

  constructor(type: string, isComplete: boolean = false, val1?: any, val2?: any) {
    this.type = type;
    this.isComplete = isComplete;
    this.val1 = val1 ?? undefined;
    this.val2 = val2 ?? undefined;
  }
}
