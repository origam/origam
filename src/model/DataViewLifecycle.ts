import { Machine, interpret } from "xstate";
import { createAtom, flow, computed, action } from "mobx";
import {
  IDataViewLifecycle,
  CDataViewLifecycle
} from "./types/IDataViewLifecycle";
import { getApi } from "./selectors/getApi";
import { getMenuItemId } from "./selectors/getMenuItemId";
import { getDataStructureEntityId } from "./selectors/DataView/getDataStructureEntityId";
import { getColumnNamesToLoad } from "./selectors/DataView/getColumnNamesToLoad";
import { getDataTable } from "./selectors/DataView/getDataTable";

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

  *loadData() {
    const api = getApi(this);
    const loadedData = yield api.getEntities({
      MenuId: getMenuItemId(this),
      DataStructureEntityId: getDataStructureEntityId(this),
      Ordering: [],
      ColumnNames: getColumnNamesToLoad(this),
      Filter: ""
    });
    const dataTable = getDataTable(this);
    dataTable.setRecords(loadedData);
    this.interpreter.send(dataLoaded);
  }

  @action.bound
  loadFresh(): void {
    this.interpreter.send(loadData);
  }

  @computed get isWorking(): boolean {
    return this.state.value !== sIdle;
  }


  @action.bound
  run(): void {
    this.interpreter.start();
  }

  parent?: any;
}
