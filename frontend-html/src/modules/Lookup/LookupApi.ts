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
import { IApi } from "model/entities/types/IApi";

export class LookupApi {
  constructor(private api: () => IApi) {
  }

  async getLookupLabels(request: Map<string, Map<any, any>>) {
    const requestRaw: any[] = [];
    for (let [lookupId, v1] of request) {
      requestRaw.push({
        LookupId: lookupId,
        MenuId: undefined,
        LabelIds: Array.from(v1.keys()),
      });
    }

    const resultRaw: { [k: string]: any } = await this.api().getLookupLabelsEx(requestRaw);
    const result = new Map<string, Map<any, any>>();
    for (let [lookupId, lookupResolved] of Object.entries(resultRaw)) {
      if (!result.has(lookupId)) {
        result.set(lookupId, new Map());
      }
      const lookupMap = result.get(lookupId)!;
      for (let [labelId, labelValue] of Object.entries(lookupResolved)) {
        lookupMap.set(labelId, labelValue);
      }
    }
    return result;
  }
}

export const ILookupApi = TypeSymbol<LookupApi>("ILookupApi");
