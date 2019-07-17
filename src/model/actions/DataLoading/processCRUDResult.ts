import { getDataViewByEntity } from "../../selectors/DataView/getDataViewByEntity";
import { runInAction } from "mobx";
export enum IResponseOperation {
  DeleteAllData = -2,
  Delete = -1,
  Update = 0,
  Create = 1,
  FormSaved = 2,
  FormNeedsRefresh = 3,
  CurrentRecordNeedsUpdate = 4,
  RefreshPortal = 5
}

export interface ICRUDResult {
  entity: string;
  objectId: string;
  operation: IResponseOperation;
  requestingGrid: string | null;
  state: string | null;
  wrappedObject: any[];
}

export function processCRUDResult(ctx: any, result: ICRUDResult) {
  runInAction(() => {
    // for (let resultItem of result) {
    // TODO: Can it be an array?
    const resultItem = result;
    switch (resultItem.operation) {
      case IResponseOperation.Update: {
        const dataView = getDataViewByEntity(ctx, resultItem.entity);
        if (dataView) {
          dataView.dataTable.substituteRecord(resultItem.wrappedObject);
          dataView.dataTable.clearRecordDirtyValues(resultItem.objectId);
        }
        break;
      }
      default:
        throw new Error("Unknown operation " + resultItem.operation);
    }
    //}
  });
}
