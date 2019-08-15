import { action, createAtom, flow, when } from "mobx";
import { processCRUDResult } from "model/actions/DataLoading/processCRUDResult";
import { getDataViewByEntity } from "model/selectors/DataView/getDataViewByEntity";
import { getMenuItemType } from "model/selectors/getMenuItemType";
import { map2obj } from "utils/objects";
import { interpretScreenXml } from "xmlInterpreters/screenXml";
import { interpret, Machine } from "xstate";
import { loadFreshData } from "../../actions/DataView/loadFreshData";
import { getFormScreen } from "../../selectors/FormScreen/getFormScreen";
import { getApi } from "../../selectors/getApi";
import { getMenuItemId } from "../../selectors/getMenuItemId";
import { getOpenedScreen } from "../../selectors/getOpenedScreen";
import { getSessionId } from "../../selectors/getSessionId";
import { IFormScreenLifecycle } from "../types/IFormScreenLifecycle";
import {
  onCreateRow,
  onCreateRowDone,
  onDeleteRow,
  onFlushData,
  onFlushDataDone,
  onInitUIDone,
  onDeleteRowDone
} from "./constants";
import { FormScreenDef } from "./FormScreenDef";

export class FormScreenLifecycle implements IFormScreenLifecycle {
  $type_IFormScreenLifecycle: 1 = 1;

  machine = Machine(FormScreenDef, {
    services: {
      initUI: (ctx, event) => (send, onEvent) => flow(this.initUI.bind(this))(),
      flushData: (ctx, event) => (send, onEvent) =>
        flow(this.flushData.bind(this))(),
      createRow: (ctx, event) => (send, onEvent) =>
        flow(this.createRow.bind(this))(event.entity, event.gridId),
      deleteRow: (ctx, event) => (send, onEvent) =>
        flow(this.deleteRow.bind(this))(event.entity, event.rowId)
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
    const openedScreen = getOpenedScreen(this);
    const menuItemId = getMenuItemId(this);
    const menuItemType = getMenuItemType(this);
    const initUIResult = yield api.initUI({
      Type: menuItemType,
      ObjectId: menuItemId,
      FormSessionId: undefined,
      IsNewSession: true,
      RegisterSession: true,
      DataRequested: !openedScreen.dontRequestData
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

  *createRow(entity: string, gridId: string) {
    const api = getApi(this);
    const createObjectResult = yield api.createObject({
      SessionFormIdentifier: getSessionId(this),
      Entity: entity,
      RequestingGridId: gridId,
      Values: {},
      Parameters: {}
    });
    console.log(createObjectResult);
    processCRUDResult(this, createObjectResult);
    this.interpreter.send(onCreateRowDone);
  }

  *deleteRow(entity: string, rowId: string) {
    const api = getApi(this);
    const deleteObjectResult = yield api.deleteObject({
      SessionFormIdentifier: getSessionId(this),
      Entity: entity,
      Id: rowId
    });
    console.log(deleteObjectResult);
    processCRUDResult(this, deleteObjectResult);
    this.interpreter.send(onDeleteRowDone);
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

  @action.bound
  onCreateRow(entity: string, gridId: string): void {
    this.interpreter.send({ type: onCreateRow, entity, gridId });
  }

  @action.bound
  onDeleteRow(entity: string, rowId: string): void {
    this.interpreter.send({ type: onDeleteRow, entity, rowId });
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
