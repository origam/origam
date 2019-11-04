import { action, createAtom, flow, reaction, computed, autorun } from "mobx";
import { navigateAsChild } from "model/actions/DataView/navigateAsChild";
import { getBindingChildren } from "model/selectors/DataView/getBindingChildren";
import { getDataView } from "model/selectors/DataView/getDataView";
import { getDataViewLabel } from "model/selectors/DataView/getDataViewLabel";
import { getEntity } from "model/selectors/DataView/getEntity";
import { getIsBindingParent } from "model/selectors/DataView/getIsBindingParent";
import { getIsBindingRoot } from "model/selectors/DataView/getIsBindingRoot";
import { getMasterRowId } from "model/selectors/DataView/getMasterRowId";
import { getParentRowId } from "model/selectors/DataView/getParentRowId";
import { getDontRequestData } from "model/selectors/getDontRequestData";
import { getSessionId } from "model/selectors/getSessionId";
import { interpret, Machine } from "xstate";
import { getApi } from "../../selectors/getApi";
import { getSelectedRowId } from "../../selectors/TablePanelView/getSelectedRowId";
import {
  DataViewLifecycleDef,
  onChangeMasterRow,
  onChangeMasterRowDone,
  onChangeMasterRowFailed,
  onLoadGetData,
  onLoadGetDataDone,
  onLoadGetDataFailed,
  sIdle
} from "./DataViewLifecycleDef";
import { IDataViewLifecycle } from "./types/IDataViewLifecycle";
import { errDialogSvc } from "../ErrorDialog";
import _ from "lodash";

export class DataViewLifecycle implements IDataViewLifecycle {
  
  $type_IDataViewLifecycle: 1 = 1;

  machine = Machine(DataViewLifecycleDef, {
    services: {
      changeMasterRow: (ctx, event) => (send, onEvent) =>
        flow(this.flChangeMasterRow.bind(this))(),
      loadGetData: (ctx, event) => (send, onEvent) =>
        flow(this.flLoadGetData.bind(this))(),
      ...errDialogSvc(this)
    },
    actions: {
      navigateChildren: (ctx, event) => this.navigateChildren()
    }
  });

  constructor() {
    autorun(() => {
      console.log('DataViewLoading:', this.isWorking)
    })
  }

  stateAtom = createAtom("formScreenLifecycleState");
  interpreter = interpret(this.machine).onTransition((state, event) => {
    console.log("DataView lifecycle:", state, event);
    this.stateAtom.reportChanged();
  });

  get state() {
    this.stateAtom.reportObserved();
    return this.interpreter.state;
  }

  @computed get isWorking() {
    return !this.state.matches(sIdle)
  }


  *flChangeMasterRow() {
    try {
      const api = getApi(this);
      yield api.setMasterRecord({
        SessionFormIdentifier: getSessionId(this),
        Entity: getEntity(this),
        RowId: getSelectedRowId(this)!
      });
      this.interpreter.send(onChangeMasterRowDone);
    } catch (error) {
      console.error(error);
      this.interpreter.send({ type: onChangeMasterRowFailed, error });
    }
  }

  *flLoadGetData() {
    try {
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
    } catch (error) {
      console.error(error);
      this.interpreter.send({ type: onLoadGetDataFailed, error });
    }
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
  
  @action.bound changeMasterRowImm() {
    this.interpreter.send(onChangeMasterRow);
  }

  changeMasterRow = _.debounce(this.changeMasterRowImm, 100);

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
    if (getDontRequestData(this)) {
      console.log(" - starting parent row reaction");
      this.disposers.push(this.startSelectedRowReaction());
    }
  }

  parent?: any;
}
