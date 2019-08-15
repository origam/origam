import { action, createAtom, flow, when, _getGlobalState } from "mobx";
import { interpret, Machine } from "xstate";
import { loadFreshData } from "../../actions/DataView/loadFreshData";
import { getFormScreen } from "../../selectors/FormScreen/getFormScreen";
import { getApi } from "../../selectors/getApi";
import { IFormScreenLifecycle } from "../types/IFormScreenLifecycle";
import { FormScreenDef } from "./FormScreenDef";
import { getMenuItemId } from "../../selectors/getMenuItemId";
import { onInitUIDone, onFlushData, onFlushDataDone } from "./constants";
import { getMenuItemType } from "model/selectors/getMenuItemType";
import { interpretScreenXml } from "xmlInterpreters/screenXml";
import { getDataViewList } from "model/selectors/FormScreen/getDataViewList";
import { IOpenedScreen } from "../types/IOpenedScreen";
import { getOpenedScreen } from "../../selectors/getOpenedScreen";
import { getDataViewByEntity } from "model/selectors/DataView/getDataViewByEntity";
import { getSessionId } from "../../selectors/getSessionId";
import { map2obj } from "utils/objects";
import { processCRUDResult } from "model/actions/DataLoading/processCRUDResult";

export class FormScreenLifecycle implements IFormScreenLifecycle {
  $type_IFormScreenLifecycle: 1 = 1;

  machine = Machine(FormScreenDef, {
    services: {
      initUI: (ctx, event) => (send, onEvent) => flow(this.initUI.bind(this))(),
      flushData: (ctx, event) => (send, onEvent) =>
        flow(this.flushData.bind(this))()
    },
    actions: {
      applyInitUIResult: (ctx, event) => this.applyInitUIResult(event as any)
    }
  });

  stateAtom = createAtom("formScreenLifecycleState");
  interpreter = interpret(this.machine).onTransition((state, event) => {
    console.log("FormScreen lifecycle:", state, event);
    this.stateAtom.reportChanged();
  });

  get state() {
    this.stateAtom.reportObserved();
    return this.interpreter.state;
  }

  *initUI() {
    const api = getApi(this);
    const menuItemId = getMenuItemId(this);
    const menuItemType = getMenuItemType(this);
    const initUIResult = yield api.initUI({
      Type: menuItemType,
      ObjectId: menuItemId,
      FormSessionId: undefined,
      IsNewSession: true,
      RegisterSession: true,
      DataRequested: true
    });
    console.log(initUIResult);
    this.interpreter.send({ type: onInitUIDone, initUIResult });
  }

  *flushData() {
    const api = getApi(this);
    for (let dataView of getFormScreen(this).dataViews) {
      for (let row of dataView.dataTable.getDirtyValueRows()) {
        const updateObjectResult = yield api.updateObject({
          SessionFormIdentifier: getSessionId(this),
          Entity: dataView.entity,
          Id: dataView.dataTable.getRowId(row),
          Values: map2obj(dataView.dataTable.getDirtyValues(row))
        });
        console.log(updateObjectResult);
        processCRUDResult(this, updateObjectResult);
      }
    }
    this.interpreter.send(onFlushDataDone);
  }

  @action.bound
  applyInitUIResult(args: { initUIResult: any }) {
    const openedScreen = getOpenedScreen(this);
    const screenXmlObj = args.initUIResult.formDefinition;
    const screen = interpretScreenXml(
      screenXmlObj,
      this,
      args.initUIResult.sessionId
    );
    openedScreen.setContent(screen);
    for (let [entityKey, entityValue] of Object.entries(
      args.initUIResult.data
    )) {
      console.log(entityKey, entityValue);
      const dataView = getDataViewByEntity(screen, entityKey);
      if (dataView) {
        dataView.dataTable.setRecords((entityValue as any).data);
      }
    }
  }

  @action.bound
  onFlushData(): void {
    this.interpreter.send(onFlushData);
  }

  *loadDataViews() {
    const screen = getFormScreen(this);
    screen.rootDataViews.forEach(s => loadFreshData(s));
    yield when(() => screen.rootDataViews.every(dv => !dv.isWorking));
  }

  @action.bound
  run(): void {
    this.interpreter.start();
  }

  parent?: any;
}
