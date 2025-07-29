/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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
import { IDataView } from "./types/IDataView";
import { getApi } from "../selectors/getApi";
import { getSessionId } from "../selectors/getSessionId";
import { getParentRowId } from "../selectors/DataView/getParentRowId";
import { getMasterRowId } from "../selectors/DataView/getMasterRowId";

export class DataViewCache {
  constructor(private ctx: any) {
  }

  dataMap = new Map<string, any[][]>();

  public UpdateData(dataView: IDataView) {
    const parentRowId = getParentRowId(dataView);
    const masterRowId = getMasterRowId(dataView);
    if (!parentRowId || !masterRowId) {
      return;
    }
    const cacheKey = this.makeCacheKey(dataView.modelInstanceId, parentRowId, masterRowId);
    this.dataMap.set(cacheKey, [...dataView.dataTable.allRows])
  }

  public async getData(args: { childEntity: string, modelInstanceId: string, parentRecordId: string, rootRecordId: string }) {
    const cacheKey = this.makeCacheKey(args.modelInstanceId, args.parentRecordId, args.rootRecordId);
    if (!this.dataMap.has(cacheKey)) {
      const rows = await this.callGetData(args.childEntity, args.parentRecordId, args.rootRecordId);
      this.dataMap.set(cacheKey, rows);
    }
    return this.dataMap.get(cacheKey);
  }

  public clear() {
    this.dataMap.clear();
  }

  private callGetData(childEntity: string, parentRecordId: string, rootRecordId: string) {
    const api = getApi(this.ctx);
    const dataPromise = api.getData({
      SessionFormIdentifier: getSessionId(this.ctx),
      ChildEntity: childEntity,
      ParentRecordId: parentRecordId,
      RootRecordId: rootRecordId,
    });
    return dataPromise;
  }

  private makeCacheKey(childEntity: string, parentRecordId: string, rootRecordId: string) {
    return childEntity + "_" + parentRecordId + "_" + rootRecordId;
  }
}