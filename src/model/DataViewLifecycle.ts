import { Machine, interpret } from "xstate";
import { createAtom, flow, computed, action, autorun, reaction } from "mobx";
import {
  IDataViewLifecycle,
  CDataViewLifecycle
} from "./types/IDataViewLifecycle";
import { getApi } from "./selectors/getApi";
import { getMenuItemId } from "./selectors/getMenuItemId";
import { getDataStructureEntityId } from "./selectors/DataView/getDataStructureEntityId";
import { getColumnNamesToLoad } from "./selectors/DataView/getColumnNamesToLoad";
import { getDataTable } from "./selectors/DataView/getDataTable";
import { getDataView } from "./selectors/DataView/getDataView";
import { getComponentBindingChildren } from "./selectors/DataView/getComponentBindingChildren";
import { getDataViewLifecycle } from "./selectors/DataView/getDataViewLifecycle";
import { isValidRowSelection } from "./selectors/DataView/isValidRowSelection";
import { getIsBindingRoot } from "./selectors/DataView/getIsBindingRoot";
import { getMasterRowId } from "./selectors/DataView/getMasterRowId";

export const loadData = "loadData";
export const dataLoaded = "dataLoaded";

export const sLoadData = "sLoadData";
export const sIdle = "sIdle";

export class DataViewLifecycle implements IDataViewLifecycle {
  $type: typeof CDataViewLifecycle = CDataViewLifecycle;

  machine = Machine(
    {
      initial: sIdle,
      states: {
        [sIdle]: {
          on: {
            [loadData]: sLoadData
          }
        },
        [sLoadData]: {
          invoke: { src: "loadData" },
          on: {
            [dataLoaded]: sIdle
          }
        }
      }
    },
    {
      services: {
        loadData: (ctx, event) => (send, onEvent) =>
          flow(this.loadData.bind(this))()
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

  @action.bound
  loadFresh(): void {
    this.interpreter.send(loadData);
  }

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
          !getDataView(this).isAnyBindingAncestorWorking
        ) {
          this.loadFresh();
        } else {
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
