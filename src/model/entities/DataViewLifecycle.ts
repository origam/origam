import { Machine, interpret } from "xstate";
import {
  createAtom,
  flow,
  computed,
  action,
  autorun,
  reaction,
  comparer
} from "mobx";
import { IDataViewLifecycle } from "./types/IDataViewLifecycle";
import { getApi } from "../selectors/getApi";
import { getMenuItemId } from "../selectors/getMenuItemId";
import { getDataStructureEntityId } from "../selectors/DataView/getDataStructureEntityId";
import { getColumnNamesToLoad } from "../selectors/DataView/getColumnNamesToLoad";
import { getDataTable } from "../selectors/DataView/getDataTable";
import { getDataView } from "../selectors/DataView/getDataView";
import { getIsBindingRoot } from "../selectors/DataView/getIsBindingRoot";
import { getMasterRowId } from "../selectors/DataView/getMasterRowId";
import { IProperty } from "./types/IProperty";
import { map2obj } from "../../utils/objects";
import { processCRUDResult } from "../actions/DataLoading/processCRUDResult";
import { getSelectedRow } from "../selectors/DataView/getSelectedRow";
import { getSelectedRowId } from "../selectors/TablePanelView/getSelectedRowId";
import { when } from "mobx";
import { getRowCount } from "../selectors/TablePanelView/getRowCount";

export const loadData = "loadData";
export const flushData = "flushData";
export const createRow = "createRow";
export const dataLoaded = "dataLoaded";
export const dataFlushed = "dataFlushed";
export const rowCreated = "rowCreated";

export const sLoadData = "sLoadData";
export const sFlushData = "sFlushData";
export const sIdle = "sIdle";
export const sCreateRow = "sCreateRow";

export class DataViewLifecycle implements IDataViewLifecycle {
  $type_IDataViewLifecycle: 1 = 1;

  machine = Machine(
    {
      initial: sIdle,
      states: {
        [sIdle]: {
          on: {
            [loadData]: sLoadData,
            [flushData]: sFlushData,
            [createRow]: sCreateRow
          }
        },
        [sLoadData]: {
          invoke: { src: "loadData" },
          on: {
            [dataLoaded]: sIdle
          }
        },
        [sFlushData]: {
          invoke: { src: "flushData" },
          on: {
            [dataFlushed]: sIdle
          }
        },
        [sCreateRow]: {
          invoke: { src: "createRow" },
          on: {
            [rowCreated]: sIdle
          }
        }
      }
    },
    {
      services: {
        loadData: (ctx, event) => (send, onEvent) =>
          flow(this.loadData.bind(this))(),
        flushData: (ctx, { row, property }) => (send, onEvent) =>
          flow(this.flushData.bind(this))(),
        createRow: (ctx, event) => (send, onEvent) =>
          flow(this.createRow.bind(this))()
      }
    }
  );

  stateAtom = createAtom("formScreenLifecycleState");
  interpreter = interpret(this.machine).onTransition((state, event) => {
    console.log("DataView lifecycle:", state, event);
    this.stateAtom.reportChanged();
  });

  get state() {
    this.stateAtom.reportObserved();
    return this.interpreter.state;
  }

  @computed get isWorking(): boolean {
    return this.state.value !== sIdle;
  }

  @computed get bindingControllersForMe() {
    return getDataView(this).parentBindings.map(b => b.bindingController);
  }

  @computed get isParentBindingsValid() {
    return getDataView(this).parentBindings.every(
      binding => binding.isBindingControllerValid
    );
  }

  @computed get dataFilter() {
    const bindingConstraint = this.bindingControllersForMe[0];
    if (!bindingConstraint) {
      return "";
    }
    let bindingFilter = [];
    if (bindingConstraint.length === 1) {
      bindingFilter = [bindingConstraint[0][0], "eq", bindingConstraint[0][1]];
    } else if (bindingConstraint.length > 1) {
      bindingFilter = ["$AND"];
      for (let bc of bindingConstraint) {
        bindingFilter.push(bc[0], "eq", bc[1]);
      }
    }
    if (bindingFilter.length === 0) {
      return "";
    } else {
      return JSON.stringify(bindingFilter);
    }
  }

  @computed get masterRowId() {
    if (getIsBindingRoot(this)) {
      return undefined;
    } else {
      return getMasterRowId(this);
    }
  }

  *loadData() {
    const api = getApi(this);
    const loadedData = yield api.getEntities({
      MenuId: getMenuItemId(this),
      DataStructureEntityId: getDataStructureEntityId(this),
      Ordering: [],
      ColumnNames: getColumnNamesToLoad(this),
      Filter: this.dataFilter,
      MasterRowId: this.masterRowId
    });
    const dataTable = getDataTable(this);
    dataTable.setRecords(loadedData);
    getDataView(this).selectFirstRow();
    this.interpreter.send(dataLoaded);
  }

  *flushData() {
    const api = getApi(this);
    const dataTable = getDataTable(this);
    for (let row of dataTable.getDirtyValueRows()) {
      const rowId = dataTable.getRowId(row);
      const dirtyValues = dataTable.getDirtyValues(row);
      console.log("Updating", rowId);
      const result = yield api.putEntity({
        MenuId: getMenuItemId(this),
        DataStructureEntityId: getDataStructureEntityId(this),
        RowId: rowId,
        NewValues: map2obj(dirtyValues)
      });
      console.log("...Updated.");

      processCRUDResult(this, result);
    }

    for (let row of dataTable.getDirtyDeletedRows()) {
      const RowId = dataTable.getRowId(row);
      console.log("Deleting", RowId);
      const result = yield api.deleteEntity({
        MenuId: getMenuItemId(this),
        DataStructureEntityId: getDataStructureEntityId(this),
        RowIdToDelete: RowId
      });
      console.log("...Deleted.");
      // processCRUDResult(this, result);
      dataTable.deleteRow(row);
    }

    /*
      const newRecord = Array(row.length) as any[];
      for (let prop of dataTable.properties) {
        newRecord[prop.dataIndex] = result.wrappedObject[prop.dataSourceIndex];
      }
      // this.dataTable.substRecord(RowId, newRecord);
      // this.dataTable.removeDirtyRow(RowId);
    }
    /*for (let RowId of this.dataTable.dirtyDeletedIds.keys()) {
      console.log("Deleting", RowId);
      const result = await this.api.deleteEntity({
        MenuId: this.menuItemId,
        DataStructureEntityId: this.dataStructureEntityId,
        RowIdToDelete: RowId
      });
      console.log("...Deleted.");
      runInAction(() => {
        this.dataTable.removeDirtyDeleted(RowId);
        this.dataTable.removeRow(RowId);
      });
    }
    for (let [RowId, values] of this.dataTable.dirtyValues) {
      console.log("Updating", RowId);
      const result = await this.api.putEntity({
        MenuId: this.menuItemId,
        DataStructureEntityId: this.dataStructureEntityId,
        RowId,
        NewValues: values
      });
      console.log("...Updated.");
      runInAction(() => {
        const newRecord = Array(this.dataTable.properties.count) as IRecord;
        for (let prop of this.dataTable.properties.items) {
          newRecord[prop.dataIndex] =
            result.wrappedObject[prop.dataSourceIndex];
        }
        this.dataTable.substRecord(RowId, newRecord);
        this.dataTable.removeDirtyRow(RowId);
      });
    }*/
    this.interpreter.send(dataFlushed);
  }

  *createRow() {
    const api = getApi(this);
    const result = yield api.newEntity({
      DataStructureEntityId: getDataStructureEntityId(this),
      MenuId: getMenuItemId(this)
    });
    console.log(result);
    processCRUDResult(this, result);
    this.interpreter.send(rowCreated)
  }

  @action.bound
  loadFresh(): void {
    this.interpreter.send(loadData);
  }

  @action.bound
  requestFlushData(): void {
    this.interpreter.send(flushData);
  }

  @action.bound
  onAddRowClicked() {
    const self = this;
    return flow(function*() {
      const dataTable = getDataTable(self);
      const rowCount = getRowCount(self);
      const row = getSelectedRow(self);
      // TODO: Handle a case when row is not selected?
      if (!row && rowCount !== 0) return;
      self.interpreter.send(createRow);
      return 0;
    })();
  }

  @action.bound
  onDeleteRowClicked() {
    const self = this;
    return flow(function*() {
      const row = getSelectedRow(self);
      if (row) {
        const dataTable = getDataTable(self);
        const nearestRow = dataTable.getNearestRow(row);
        dataTable.setDirtyDeleted(row);
        self.requestFlushData();
        yield when(() => !self.isWorking);
        // TODO: Handle error case here (do not select different row):
        if (nearestRow) {
          getDataView(self).selectRow(nearestRow);
        }
      }
    })();
  }

  lastAttemptedBindingControllersForMe: any;
  parentChangeReaction() {
    return reaction(
      () => [
        this.bindingControllersForMe,
        getDataView(this).isAnyBindingAncestorWorking
      ],
      () => {
        console.log(
          "Binding controllers changed:",
          this.bindingControllersForMe
        );
        if (
          this.masterRowId &&
          !comparer.structural(
            this.bindingControllersForMe,
            this.lastAttemptedBindingControllersForMe
          ) &&
          !getDataView(this).isAnyBindingAncestorWorking
        ) {
          this.lastAttemptedBindingControllersForMe = this.bindingControllersForMe;
          this.loadFresh();
        } else if (!this.masterRowId || !this.isParentBindingsValid) {
          getDataTable(this).clear();
        }
      }
    );
  }

  @action.bound
  run(): void {
    this.interpreter.start();
    if (!getDataView(this).isBindingRoot) {
      this.disposers.push(this.parentChangeReaction());
    }
  }

  disposers: Array<() => void> = [];

  parent?: any;
}
