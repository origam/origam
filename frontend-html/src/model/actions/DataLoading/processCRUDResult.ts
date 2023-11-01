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

import { getDataViewsByEntity } from "../../selectors/DataView/getDataViewsByEntity";
import _ from "lodash";
import { getFormScreen } from "model/selectors/FormScreen/getFormScreen";
import { putRowStateValue } from "../RowStates/putRowStateValue";
import { reloadScreen } from "../FormScreen/reloadScreen";
import { getDataViewList } from "model/selectors/FormScreen/getDataViewList";
import { getIsBindingRoot } from "model/selectors/DataView/getIsBindingRoot";
import { getWorkbench } from "model/selectors/getWorkbench";
import { getDataSources } from "model/selectors/DataSources/getDataSources";
import { IDataView } from "model/entities/types/IDataView";
import { getDataViewCache } from "../../selectors/FormScreen/getDataViewCache";
import { getFocusManager } from "model/selectors/getFocusManager";
import {flow, runInAction} from "mobx";
import {onFieldChangeG} from "../../actions-ui/DataView/TableView/onFieldChange";
import {runGeneratorInFlowWithHandler} from "../../../utils/runInFlowWithHandler";
import {saveColumnConfigurations} from "../DataView/TableView/saveColumnConfigurations";

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

export function*processCRUDResult(ctx: any, result: ICRUDResult,
                                  resortTables?: boolean | undefined,
                                  sourceDataView?: IDataView): Generator {
  if (_.isArray(result)) {
    const updates = [];
    for (let resultItem of result) {
      if (resultItem.operation === IResponseOperation.Update) {
        updates.push(resultItem);
      }
      else {
        yield*processCRUDResult(ctx, resultItem, resortTables, sourceDataView);
      }
    }
    const updatesByEntity = updates.groupBy(resultItem => resultItem.entity);
    for(const updates of updatesByEntity.values()){
      yield runGeneratorInFlowWithHandler({
        ctx: ctx,
        generator: function*() {
          yield*batchProcessUpdates(ctx, updates, resortTables);
        }()
      });
      // yield*batchProcessUpdates(ctx, updates, resortTables);
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
    // case IResponseOperation.Update: {
    //   const dataViews = getDataViewsByEntity(ctx, resultItem.entity);
    //   for (let dataView of dataViews) {
    //     dataView.dataTable.clearRecordDirtyValues(resultItem.objectId, resultItem.wrappedObject);
    //     dataView.substituteRecord(resultItem.wrappedObject);
    //     getDataViewCache(dataView).UpdateData(dataView);
    //     if (resortTables) {
    //       yield dataView.dataTable.updateSortAndFilter({retainPreviousSelection: true});
    //     }
    //     if (!dataView.selectedRow) {
    //       dataView.reselectOrSelectFirst();
    //     }
    //     dataView.formFocusManager.stopAutoFocus();
    //   }
    //   const screenFocusManager = getFocusManager(ctx);
    //   screenFocusManager.setFocus();
    //   getFormScreen(ctx).setDirty(true);
    //   break;
    // }
    case IResponseOperation.Create: {
      const dataViews = getDataViewsByEntity(ctx, resultItem.entity);
      for (let dataView of dataViews) {
        const tablePanelView = dataView.tablePanelView;
        const dataSourceRow = result.wrappedObject;
        const shouldLockNewRowAtTop = sourceDataView?.modelInstanceId === dataView.modelInstanceId;

        if (dataView.isLazyLoading) {
          yield dataView.insertRecord(tablePanelView.firstVisibleRowIndex, dataSourceRow, shouldLockNewRowAtTop);
          try {
            dataView.lifecycle.stopSelectedRowReaction();
            dataView.selectRow(dataSourceRow);
            yield*dataView.lifecycle.changeMasterRow();
          } finally {
            dataView.lifecycle.startSelectedRowReaction();
          }
        } else {
          yield dataView.insertRecord(dataView.tableRows.length, dataSourceRow, shouldLockNewRowAtTop);
          dataView.selectRow(dataSourceRow);
        }
        getDataViewCache(dataView).UpdateData(dataView);
      }
      getFormScreen(ctx).setDirty(true);
      break;
    }
    case IResponseOperation.Delete: {
      const dataViews = getDataViewsByEntity(ctx, resultItem.entity);
      for (let dataView of dataViews) {
        const row = dataView.dataTable.getRowById(resultItem.objectId);
        if (row) {
          dataView.deleteRowAndSelectNext(row);
          getDataViewCache(dataView).UpdateData(dataView);
        }
      }
      getFormScreen(ctx).setDirty(true);
      break;
    }
    case IResponseOperation.FormSaved: {
      getFormScreen(ctx).setDirty(false);
      const workbench = getWorkbench(ctx);
      const {cacheDependencies, lookupCleanerReloaderById} = workbench.lookupMultiEngine;
      const dataSources = getDataSources(ctx);
      const collectedLookupIds = cacheDependencies.getDependencyLookupIdsByCacheKeys(
        dataSources.map((ds) => ds.lookupCacheKey)
      );
      for (let lookupId of collectedLookupIds) {
        lookupCleanerReloaderById.get(lookupId)?.reloadLookupLabels();
        workbench.lookupListCache.deleteLookup(lookupId);
      }
      break;
    }
    case IResponseOperation.CurrentRecordNeedsUpdate: {
      // TODO: Throw away all data and force further navigation / throw away all rowstates
      const dataViews = getDataViewList(ctx);
      for (let dataView of dataViews) {
        if (getIsBindingRoot(dataView) && dataView.requestDataAfterSelectionChange) {
          yield*dataView.lifecycle.changeMasterRow();
          yield*dataView.lifecycle.navigateChildren();
        }
        if (!dataView.selectedRow) {
          dataView.reselectOrSelectFirst();
        }
      }
      break;
    }
    case IResponseOperation.FormNeedsRefresh: {
      if (!getFormScreen(ctx).suppressRefresh) {
        yield*reloadScreen(ctx)(); // TODO: It is not possible to reload data... Has to be done by different API endpoint
      }
      break;
    }
    case IResponseOperation.DeleteAllData: {
      const dataViews = getDataViewList(ctx);
      for (let dataView of dataViews) {
        dataView.clear();
      }
      break;
    }
    default:
      throw new Error("Unknown operation " + resultItem.operation);
  }
  //}
}

function*batchProcessUpdates(ctx: any, updates: ICRUDResult[], resortTables?: boolean | undefined,){
  const dataViews = getDataViewsByEntity(ctx, updates[0].entity);
  for (let dataView of dataViews) {
    for (let resultItem of updates){
      dataView.dataTable.clearRecordDirtyValues(resultItem.objectId, resultItem.wrappedObject);
      dataView.substituteRecord(resultItem.wrappedObject);
    }
    getDataViewCache(dataView).UpdateData(dataView);
    if (resortTables) {
      yield dataView.dataTable.updateSortAndFilter({retainPreviousSelection: true});
    }
    if (!dataView.selectedRow) {
      dataView.reselectOrSelectFirst();
    }
    dataView.formFocusManager.stopAutoFocus();
  }
  const screenFocusManager = getFocusManager(ctx);
  screenFocusManager.setFocus();
  getFormScreen(ctx).setDirty(true);
}
