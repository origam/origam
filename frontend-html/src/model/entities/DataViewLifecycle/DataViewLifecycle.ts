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

import Axios from "axios";
import { action, flow } from "mobx";
import { navigateAsChild } from "model/actions/DataView/navigateAsChild";
import { handleError } from "model/actions/handleError";
import { getBindingChildren } from "model/selectors/DataView/getBindingChildren";
import { getDataView } from "model/selectors/DataView/getDataView";
import { getEntity } from "model/selectors/DataView/getEntity";
import { getIsBindingParent } from "model/selectors/DataView/getIsBindingParent";
import { getIsBindingRoot } from "model/selectors/DataView/getIsBindingRoot";
import { getMasterRowId } from "model/selectors/DataView/getMasterRowId";
import { getParentRowId } from "model/selectors/DataView/getParentRowId";
import { isLazyLoading } from "model/selectors/isLazyLoading";
import { getSessionId } from "model/selectors/getSessionId";
import { getApi } from "../../selectors/getApi";
import { getSelectedRowId } from "../../selectors/TablePanelView/getSelectedRowId";
import { IDataViewLifecycle } from "./types/IDataViewLifecycle";
import { processCRUDResult } from "model/actions/DataLoading/processCRUDResult";
import { IDataView } from "../types/IDataView";
import { getMenuItemId } from "../../selectors/getMenuItemId";
import { getDataStructureEntityId } from "../../selectors/DataView/getDataStructureEntityId";
import { SCROLL_ROW_CHUNK } from "gui/Workbench/ScreenArea/TableView/InfiniteScrollLoader";
import { getColumnNamesToLoad } from "../../selectors/DataView/getColumnNamesToLoad";
import { joinWithAND, toFilterItem } from "../OrigamApiHelpers";
import { FlowBusyMonitor } from "utils/flow";
import { getFormScreen } from "model/selectors/FormScreen/getFormScreen";
import { getUserFilterLookups } from "model/selectors/DataView/getUserFilterLookups";
import { runGeneratorInFlowWithHandler } from "utils/runInFlowWithHandler";

export class DataViewLifecycle implements IDataViewLifecycle {
  $type_IDataViewLifecycle: 1 = 1;
  monitor: FlowBusyMonitor = new FlowBusyMonitor();
  reactToRowChanges = true;

  get isWorking() {
    return this.monitor.isWorkingDelayed;
  }

  @action.bound
  start(): void {
    if (isLazyLoading(this)) {
      const self = this;
      runGeneratorInFlowWithHandler({
        ctx: this,
        generator: function*() {
            yield*self.startSelectedRowReaction(true);
        }()
      });
    }
  }

  *runRecordChangedReaction(this: DataViewLifecycle){
    if (!this.reactToRowChanges || !isLazyLoading(this)) {
      return;
    }
    try {
      this.monitor.inFlow++;
      if (getIsBindingRoot(this)) {
        yield*this.changeMasterRow();
        yield*this.navigateChildren();
      } else if (getIsBindingParent(this)) {
        yield*this.navigateChildren();
      }
    } catch (e) {
      // TODO: Move this method to action handler file?
      yield*handleError(this)(e);
      throw e;
    } finally {
      this.monitor.inFlow--;
    }
  }

  *startSelectedRowReaction(fireImmediately?: boolean): Generator {
    this.reactToRowChanges = true;
    if (fireImmediately) {
      yield*this.runRecordChangedReaction();
    }
  }

  @action.bound stopSelectedRowReaction() {
    const wasRunning = this.reactToRowChanges;
    this.reactToRowChanges = false;
    return wasRunning;
  }

  *navigateAsChild(rows?: any[]) {
    if (rows !== undefined && rows !== null) {
      const dataView = getDataView(this);
      yield dataView.setRecords(rows);
      dataView.selectFirstRow();
    } else {
      yield*this.loadGetData();
    }
  }

  changeMasterRowCanceller: any;

  *changeMasterRow(): any {
    try {
      this.monitor.inFlow++;
      const api = getApi(this);
      this.changeMasterRowCanceller && this.changeMasterRowCanceller();
      this.changeMasterRowCanceller = api.createCanceller();
      const crudResult = yield api.setMasterRecord(
        {
          SessionFormIdentifier: getSessionId(this),
          Entity: getEntity(this),
          RowId: getSelectedRowId(this)!
        },
        this.changeMasterRowCanceller
      );
      yield*processCRUDResult(this, crudResult);
      getFormScreen(this).clearDataCache();
    } catch (error) {
      if (Axios.isCancel(error)) {
        return;
      }
      throw error;
    } finally {
      this.monitor.inFlow--;
    }
  }

  buildDetailFilter(dataView: IDataView) {
    const filterItems = [];
    for (let binding of dataView.parentBindings) {
      const selectedRow = binding.parentDataView.selectedRow;
      if (!selectedRow) {
        continue;
      }
      for (let bindingPair of binding.bindingPairs) {
        const parentDsField = binding.parentDataView.dataSource.getFieldByName(
          bindingPair.parentPropertyId
        );
        if (!parentDsField) {
          continue;
        }
        const parentValue = binding.parentDataView.dataTable.getCellValueByDataSourceField(
          selectedRow,
          parentDsField
        );
        const childDsField = binding.childDataView.dataSource.getFieldByName(
          bindingPair.childPropertyId
        );
        if (!childDsField) {
          continue;
        }
        filterItems.push(toFilterItem(childDsField.name, "eq", parentValue));
      }
    }
    return joinWithAND(filterItems);
  }


  *loadGetData(): any {
    try {
      this.monitor.inFlow++;
      const dataView = getDataView(this);
      const api = getApi(this);
      let data;
      if (dataView.isRootEntity && !dataView.isRootGrid && isLazyLoading(this)) {
        if (dataView.parentBindings.length === 1) {
          data = [dataView.parentBindings[0].parentDataView.selectedRow];
        } else {
          data = yield api.getRows({
            MenuId: getMenuItemId(dataView),
            SessionFormIdentifier: getSessionId(this),
            DataStructureEntityId: getDataStructureEntityId(dataView),
            Filter: this.buildDetailFilter(dataView),
            FilterLookups: getUserFilterLookups(dataView),
            Ordering: [],
            RowLimit: SCROLL_ROW_CHUNK,
            MasterRowId: undefined,
            RowOffset: 0,
            Parameters: {},
            ColumnNames: getColumnNamesToLoad(dataView),
          });
        }
      } else {
        const parentRowId = getParentRowId(this);
        const masterRowId = getMasterRowId(this);
        let entity = getEntity(this);
        data = !parentRowId || !masterRowId
          ? []
          : yield getFormScreen(this).getData(entity, dataView.modelInstanceId, parentRowId, masterRowId);
      }
      yield dataView.setRecords(data);
      dataView.selectFirstRow();
    } finally {
      this.monitor.inFlow--;
    }
  }

  *navigateChildren(): Generator<any, any> {
    try {
      this.monitor.inFlow++;
      const entity = getEntity(this);
      const dataView = getDataView(this);
      yield Promise.all(
        getBindingChildren(this).map(childDataView =>
          flow(function*() {
            try {
              const childEntity = getEntity(childDataView);
              if (childEntity === entity && childDataView.isPreloaded) {
                yield*navigateAsChild(childDataView, dataView.dataTable.allRows)();
              } else {
                yield*navigateAsChild(childDataView)();
              }
            } catch (e) {
              yield*handleError(childDataView)(e);
            }
          })()
        )
      );
      /*for (let bch of getBindingChildren(this)) {
        yield navigateAsChild(bch)();
      }*/
    } finally {
      this.monitor.inFlow--;
    }
  }

  parent?: any;
}
