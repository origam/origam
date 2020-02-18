import { getDataViewsByEntity } from "../../selectors/DataView/getDataViewsByEntity";
import { runInAction } from "mobx";
import _ from "lodash";
import { getFormScreen } from "model/selectors/FormScreen/getFormScreen";
import { putRowStateValue } from "../RowStates/putRowStateValue";
import { reloadScreen } from "../FormScreen/reloadScreen";
import { getDataViewList } from "model/selectors/FormScreen/getDataViewList";
import { getIsBindingParent } from "model/selectors/DataView/getIsBindingParent";
import { getIsBindingRoot } from "model/selectors/DataView/getIsBindingRoot";

export enum IResponseOperation {
  DeleteAllData = -2,
  Delete = -1, // OK
  Update = 0, // OK
  Create = 1, // OK
  FormSaved = 2, // OK ? - TODO: Check
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

export function* processCRUDResult(ctx: any, result: ICRUDResult): Generator {
  if (_.isArray(result)) {
    for (let resultItem of result) {
      yield* processCRUDResult(ctx, resultItem);
    }
    return;
  }
  const resultItem = result;
  if (resultItem.state) {
    // TODO: Context for all CRUD ops?
    // TODO: Actions are pre data view vs state is related to entity?
    putRowStateValue(ctx)(resultItem.entity, resultItem.state);
  }
  switch (resultItem.operation) {
    case IResponseOperation.Update: {
      const dataViews = getDataViewsByEntity(ctx, resultItem.entity);
      for (let dataView of dataViews) {
        dataView.dataTable.substituteRecord(resultItem.wrappedObject);
        dataView.dataTable.clearRecordDirtyValues(resultItem.objectId);
      }
      getFormScreen(ctx).setDirty(true);
      break;
    }
    case IResponseOperation.Create: {
      const dataViews = getDataViewsByEntity(ctx, resultItem.entity);
      for (let dataView of dataViews) {
        const tablePanelView = dataView.tablePanelView;
        const dataSourceRow = result.wrappedObject;
        console.log("New row:", dataSourceRow);
        dataView.dataTable.insertRecord(
          tablePanelView.firstVisibleRowIndex,
          dataSourceRow
        );
        dataView.selectRow(dataSourceRow);
      }
      getFormScreen(ctx).setDirty(true);
      break;
    }
    case IResponseOperation.Delete: {
      const dataViews = getDataViewsByEntity(ctx, resultItem.entity);
      for (let dataView of dataViews) {
        const row = dataView.dataTable.getRowById(resultItem.objectId);
        if (row) {
          dataView.dataTable.deleteRow(row);
        }
      }
      getFormScreen(ctx).setDirty(true);
      break;
    }
    case IResponseOperation.FormSaved: {
      getFormScreen(ctx).setDirty(false);
      break;
    }
    case IResponseOperation.CurrentRecordNeedsUpdate: {
      const dataViews = getDataViewList(ctx);
      for(let dataView of dataViews) {
        if(getIsBindingRoot(dataView)) {
          yield* dataView.lifecycle.changeMasterRow();
          yield* dataView.lifecycle.navigateChildren();
        }
      }
      break;
    }
    case IResponseOperation.FormNeedsRefresh: {
      yield* reloadScreen(ctx)(); // TODO: It is not possible to reload data... Has to be done by different API endpoint
      break;
    }
    default:
      throw new Error("Unknown operation " + resultItem.operation);
  }
  //}
}
