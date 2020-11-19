import { getDataViewsByEntity } from "../../selectors/DataView/getDataViewsByEntity";
import _ from "lodash";
import { getFormScreen } from "model/selectors/FormScreen/getFormScreen";
import { putRowStateValue } from "../RowStates/putRowStateValue";
import { reloadScreen } from "../FormScreen/reloadScreen";
import { getDataViewList } from "model/selectors/FormScreen/getDataViewList";
import { getIsBindingRoot } from "model/selectors/DataView/getIsBindingRoot";
import { getWorkbench } from "model/selectors/getWorkbench";
import { getDataSources } from "model/selectors/DataSources/getDataSources";
import {runInAction} from "mobx";

export enum IResponseOperation {
  DeleteAllData = -2,
  Delete = -1, // OK
  Update = 0, // OK
  Create = 1, // OK
  FormSaved = 2, // OK ? - TODO: Check
  FormNeedsRefresh = 3,
  CurrentRecordNeedsUpdate = 4,
  RefreshPortal = 5,
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
  // console.log(result)
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
        dataView.dataTable.clearRecordDirtyValues(resultItem.objectId, resultItem.wrappedObject);
        dataView.dataTable.substituteRecord(resultItem.wrappedObject);
        dataView.dataTable.updateSortAndFilter();
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
        yield dataView.dataTable.insertRecord(tablePanelView.firstVisibleRowIndex, dataSourceRow);
        dataView.dataTable.unlockAddedRowPosition();
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
      const workbench = getWorkbench(ctx);
      const { cacheDependencies, lookupCleanerReloaderById } = workbench.lookupMultiEngine;
      const dataSources = getDataSources(ctx);
      const collectedLookupIds = cacheDependencies.getDependencyLookupIdsByCacheKeys(
        dataSources.map((ds) => ds.lookupCacheKey)
      );
      for (let lookupId of collectedLookupIds) {
        console.log("Clean+Reload:", lookupId);
        lookupCleanerReloaderById.get(lookupId)?.reloadLookupLabels();
        workbench.lookupListCache.deleteLookup(lookupId);
      }
      break;
    }
    case IResponseOperation.CurrentRecordNeedsUpdate: {
      // TODO: Throw away all data and force further navigation / throw away all rowstates
      const dataViews = getDataViewList(ctx);
      for (let dataView of dataViews) {
        if (getIsBindingRoot(dataView)) {
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
    case IResponseOperation.DeleteAllData: {
      const dataViews = getDataViewList(ctx);
      for (let dataView of dataViews) {
        dataView.dataTable.clear();
      }
      break;
    }
    default:
      throw new Error("Unknown operation " + resultItem.operation);
  }
  //}
}
