import { action, flow, reaction, createAtom } from "mobx";
import { getDataViewLabel } from "model/selectors/DataView/getDataViewLabel";
import { getEntity } from "model/selectors/DataView/getEntity";
import { getIsBindingParent } from "model/selectors/DataView/getIsBindingParent";
import { getSessionId } from "model/selectors/getSessionId";
import { Machine, interpret } from "xstate";
import { getApi } from "../../selectors/getApi";
import { getSelectedRowId } from "../../selectors/TablePanelView/getSelectedRowId";
import { IDataViewLifecycle } from "./types/IDataViewLifecycle";
import {
  DataViewLifecycleDef,
  onChangeMasterRow,
  onChangeMasterRowDone,
  onLoadGetDataDone,
  onLoadGetData
} from "./DataViewLifecycleDef";
import { getIsBindingRoot } from "model/selectors/DataView/getIsBindingRoot";
import { getBindingChildren } from "model/selectors/DataView/getBindingChildren";
import { navigateAsChild } from "model/actions/DataView/navigateAsChild";
import { getParentRowId } from "model/selectors/DataView/getParentRowId";
import { getMasterRowId } from "model/selectors/DataView/getMasterRowId";
import { getDataView } from "model/selectors/DataView/getDataView";
import { getDontRequestData } from "model/selectors/getDontRequestData";

export class DataViewLifecycle implements IDataViewLifecycle {
  $type_IDataViewLifecycle: 1 = 1;

  machine = Machine(DataViewLifecycleDef, {
    services: {
      changeMasterRow: (ctx, event) => (send, onEvent) =>
        flow(this.flChangeMasterRow.bind(this))(),
      loadGetData: (ctx, event) => (send, onEvent) =>
        flow(this.flLoadGetData.bind(this))()
    },
    actions: {
      navigateChildren: (ctx, event) => this.navigateChildren()
    }
  });

  stateAtom = createAtom("formScreenLifecycleState");
  interpreter = interpret(this.machine).onTransition((state, event) => {
    console.log("DataView lifecycle:", state, event);
    this.stateAtom.reportChanged();
  });

  get state() {
    this.stateAtom.reportObserved();
    return this.interpreter.state;
  }

  *flChangeMasterRow() {
    const api = getApi(this);
    yield api.setMasterRecord({
      SessionFormIdentifier: getSessionId(this),
      Entity: getEntity(this),
      RowId: getSelectedRowId(this)!
    });
    this.interpreter.send(onChangeMasterRowDone);
  }

  *flLoadGetData() {
    const api = getApi(this);
    const data = yield api.getData({
      SessionFormIdentifier: getSessionId(this),
      ChildEntity: getEntity(this),
      ParentRecordId: getParentRowId(this)!,
      RootRecordId: getMasterRowId(this)!
    });
    console.log(data);
    const dataView = getDataView(this);
    dataView.dataTable.clear();
    dataView.dataTable.setRecords(data);
    dataView.selectFirstRow();
    this.interpreter.send(onLoadGetDataDone);
  }

  disposers: any[] = [];

  @action.bound startSelectedRowReaction() {
    return reaction(
      () => {
        return getSelectedRowId(this);
      },
      () => {
        console.log(
          getDataViewLabel(this),
          "detected control id change",
          getSelectedRowId(this)
        );
        if (getSelectedRowId(this)) {
          if (getIsBindingRoot(this)) {
            this.changeMasterRow();
          } else if (getIsBindingParent(this)) {
            this.navigateChildren();
          }
        }
      }
    );
  }

  @action.bound changeMasterRow() {
    this.interpreter.send(onChangeMasterRow);
  }

  @action.bound navigateChildren() {
    for (let bch of getBindingChildren(this)) {
      navigateAsChild(bch)();
    }
  }

  @action.bound navigateAsChild() {
    this.interpreter.send(onLoadGetData);
  }

  @action.bound cancelNavigation() {}

  @action.bound cancelSubtreeNavigation() {}

  @action.bound
  start(): void {
    this.interpreter.start();
    console.log("Data view started:", getDataViewLabel(this));
    if (getDontRequestData(this) && getIsBindingParent(this)) {
      console.log(" - starting parent row reaction");
      this.disposers.push(this.startSelectedRowReaction());
    }
  }

  parent?: any;
}
