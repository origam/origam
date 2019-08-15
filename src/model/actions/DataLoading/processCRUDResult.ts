import { getDataViewByEntity } from "../../selectors/DataView/getDataViewByEntity";
import { runInAction } from "mobx";
import _ from "lodash";
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
    if (_.isArray(result)) {
      result.forEach(resultItem => processCRUDResult(ctx, resultItem));
      return;
    }
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
      case IResponseOperation.Create: {
        const dataView = getDataViewByEntity(ctx, resultItem.entity);
        if (dataView) {
          const tablePanelView = dataView.tablePanelView;
          const dataSourceRow = result.wrappedObject;
          console.log("New row:", dataSourceRow);
          dataView.dataTable.insertRecord(
            tablePanelView.firstVisibleRowIndex,
            dataSourceRow
          );
          dataView.selectRow(dataSourceRow);
        }
        break;
      }
      case IResponseOperation.Delete: {
        const dataView = getDataViewByEntity(ctx, resultItem.entity);
        if (dataView) {
          const row = dataView.dataTable.getRowById(resultItem.objectId);
          if (row) {
            dataView.dataTable.deleteRow(row);
          }
        }
        break;
      }
      default:
        throw new Error("Unknown operation " + resultItem.operation);
    }
    //}
  });
}
