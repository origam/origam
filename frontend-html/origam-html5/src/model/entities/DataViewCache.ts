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