import { QuestionSaveData } from "gui/Components/Dialogs/QuestionSaveData";
import { action, computed, createAtom, flow, runInAction, when } from "mobx";
import { processActionResult } from "model/actions/Actions/processActionResult";
import { closeForm } from "model/actions/closeForm";
import { processCRUDResult } from "model/actions/DataLoading/processCRUDResult";
import { IAction } from "model/entities/types/IAction";
import { getDataViewsByEntity } from "model/selectors/DataView/getDataViewsByEntity";
import { getDialogStack } from "model/selectors/getDialogStack";
import { getMenuItemType } from "model/selectors/getMenuItemType";
import React from "react";
import { map2obj } from "utils/objects";
import { interpretScreenXml } from "xmlInterpreters/screenXml";
import { interpret, Machine } from "xstate";
import { getFormScreen } from "../../selectors/FormScreen/getFormScreen";
import { getScreenParameters } from "../../selectors/FormScreen/getScreenParameters";
import { getApi } from "../../selectors/getApi";
import { getMenuItemId } from "../../selectors/getMenuItemId";
import { getOpenedScreen } from "../../selectors/getOpenedScreen";
import { getSessionId } from "../../selectors/getSessionId";
import { IFormScreenLifecycle } from "../types/IFormScreenLifecycle";
import {
  onCreateRow,
  onCreateRowDone,
  onDeleteRow,
  onDeleteRowDone,
  onExecuteAction,
  onExecuteActionDone,
  onFlushData,
  onFlushDataDone,
  onInitUIDone,
  onPerformCancel,
  onPerformNoSave,
  onPerformSave,
  onRefreshSession,
  onRefreshSessionDone,
  onRequestScreenClose,
  onSaveSession,
  onSaveSessionDone,
  sFormScreenRunning,
  onLoadDataDone
} from "./constants";
import { FormScreenDef } from "./FormScreenDef";
import { getDataStructureEntityId } from "model/selectors/DataView/getDataStructureEntityId";
import { getDataSourceFields } from "model/selectors/DataSources/getDataSourceFields";
import { getDontRequestData } from "model/selectors/getDontRequestData";
import { getColumnNamesToLoad } from "model/selectors/DataView/getColumnNamesToLoad";
import { getDataViewList } from "model/selectors/FormScreen/getDataViewList";

export class FormScreenLifecycle implements IFormScreenLifecycle {
  $type_IFormScreenLifecycle: 1 = 1;

  machine = Machine(FormScreenDef, {
    services: {
      initUI: (ctx, event) => (send, onEvent) => flow(this.initUI.bind(this))(),
      loadData: (ctx, event) => (send, onEvent) =>
        flow(this.loadData.bind(this))(),
      flushData: (ctx, event) => (send, onEvent) =>
        flow(this.flushData.bind(this))(),
      createRow: (ctx, event) => (send, onEvent) =>
        flow(this.createRow.bind(this))(event.entity, event.gridId),
      deleteRow: (ctx, event) => (send, onEvent) =>
        flow(this.deleteRow.bind(this))(event.entity, event.rowId),
      saveSession: (ctx, event) => (send, onEvent) =>
        flow(this.saveSession.bind(this))(),
      refreshSession: (ctx, event) => (send, onEvent) =>
        flow(this.refreshSession.bind(this))(),
      executeAction: (ctx, event) => (send, onEvent) =>
        flow(this.executeAction.bind(this))(
          event.gridId,
          event.entity,
          event.action,
          event.selectedItems
        ),
      questionSaveData: (ctx, event) => (send, onEvent) =>
        this.questionSaveData()
    },
    actions: {
      applyInitUIResult: (ctx, event) => this.applyInitUIResult(event as any),
      closeForm: (ctx, event) => this.closeForm()
    },
    guards: {
      isDirtySession: (ctx, event) => this.isDirtySession,
      isReadData: (ctx, event) => this.isReadData
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

  @computed get isWorking() {
    return !this.state.matches(sFormScreenRunning);
  }

  *initUI() {
    const api = getApi(this);
    const openedScreen = getOpenedScreen(this);
    const menuItemId = getMenuItemId(this);
    const menuItemType = getMenuItemType(this);
    const parameters = getScreenParameters(this);
    const initUIResult = yield api.initUI({
      Type: menuItemType,
      ObjectId: menuItemId,
      FormSessionId: undefined,
      IsNewSession: true,
      RegisterSession: true,
      DataRequested: !openedScreen.dontRequestData,
      Parameters: parameters
    });
    console.log(initUIResult);
    this.interpreter.send({ type: onInitUIDone, initUIResult });
  }

  *loadData() {
    const api = getApi(this);
    const formScreen = getFormScreen(this);
    for (let rootDataView of formScreen.rootDataViews) {
      const loadedData = yield api.getRows({
        MenuId: getMenuItemId(rootDataView),
        DataStructureEntityId: getDataStructureEntityId(rootDataView),
        Filter: "",
        Ordering: [],
        RowLimit: 10000,
        ColumnNames: getColumnNamesToLoad(rootDataView),
        MasterRowId: undefined
      });
      rootDataView.dataTable.clear();
      rootDataView.dataTable.setRecords(loadedData);
      rootDataView.selectFirstRow();

      /*for (let chb of rootDataView.childBindings) {
        yield api.getData({
          SessionFormIdentifier: getSessionId(this),
          ChildEntity: "",
          ParentRecordId: "",
          RootRecordId: ""
        });
      }*/
    }
    this.interpreter.send(onLoadDataDone);
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

  *saveSession() {
    const api = getApi(this);
    yield api.saveSessionQuery(getSessionId(this));
    const result = yield api.saveSession(getSessionId(this));
    processCRUDResult(this, result);
    this.interpreter.send(onSaveSessionDone);
  }

  *refreshSession() {
    const api = getApi(this);
    const result = yield api.refreshSession(getSessionId(this));
    this.applyData(result);
    this.interpreter.send(onRefreshSessionDone);
  }

  *executeAction(
    gridId: string,
    entity: string,
    action: IAction,
    selectedItems: string[]
  ) {
    const api = getApi(this);
    const queryResult = yield api.executeActionQuery({
      SessionFormIdentifier: getSessionId(this),
      Entity: entity,
      ActionType: action.type,
      ActionId: action.id,
      ParameterMappings: {},
      SelectedItems: selectedItems,
      InputParameters: {}
    });
    console.log("EAQ", queryResult);

    const result = yield api.executeAction({
      SessionFormIdentifier: getSessionId(this),
      Entity: entity,
      ActionType: action.type,
      ActionId: action.id,
      ParameterMappings: {},
      SelectedItems: selectedItems,
      InputParameters: {},
      RequestingGrid: gridId
    });
    console.log("EA", result);

    processActionResult(action)(result);

    this.interpreter.send(onExecuteActionDone);
  }

  @action.bound
  applyInitUIResult(args: { initUIResult: any }) {
    console.log('Apply init ui result.')
    const openedScreen = getOpenedScreen(this);
    const screenXmlObj = args.initUIResult.formDefinition;
    const screen = interpretScreenXml(
      screenXmlObj,
      this,
      args.initUIResult.sessionId
    );
    
    openedScreen.setContent(screen);
    screen.printMasterDetailTree();
    this.applyData(args.initUIResult.data);
    getDataViewList(this).forEach(dv => dv.start());
  }

  @action.bound applyData(data: any) {
    for (let [entityKey, entityValue] of Object.entries(data)) {
      console.log(entityKey, entityValue);
      const dataViews = getDataViewsByEntity(this, entityKey);
      for (let dataView of dataViews) {
        dataView.dataTable.clear();
        dataView.dataTable.setRecords((entityValue as any).data);
        dataView.selectFirstRow();
      }
    }
  }

  onFlushDataWaiting: any;
  @action.bound
  async onFlushData(): Promise<any> {
    this.onFlushDataWaiting && this.onFlushDataWaiting.cancel();
    this.isWorking &&
      (await (this.onFlushDataWaiting = when(() => !this.isWorking)));
    // TODO: Exec only when not in error?
    runInAction(() => this.interpreter.send(onFlushData));
  }

  @action.bound
  onCreateRow(entity: string, gridId: string): void {
    this.interpreter.send({ type: onCreateRow, entity, gridId });
  }

  @action.bound
  onDeleteRow(entity: string, rowId: string): void {
    this.interpreter.send({ type: onDeleteRow, entity, rowId });
  }

  @action.bound
  onSaveSession(): void {
    this.interpreter.send({ type: onSaveSession });
  }

  @action.bound
  onRefreshSession(): void {
    this.interpreter.send({ type: onRefreshSession });
  }

  @action.bound
  onRequestScreenClose(): void {
    this.interpreter.send(onRequestScreenClose);
  }

  @action.bound
  closeForm() {
    closeForm(this)();
  }

  @action.bound
  async onExecuteAction(
    gridId: string,
    entity: string,
    action: IAction,
    selectedItems: string[]
  ): Promise<any> {
    this.interpreter.send({
      type: onExecuteAction,
      gridId,
      entity,
      action,
      selectedItems
    });
    await when(() => !this.isWorking);
  }



  @action.bound questionSaveData() {
    return getDialogStack(this).pushDialog(
      "",
      <QuestionSaveData
        screenTitle={"SCREEN_TITLE"}
        onSaveClick={() => this.interpreter.send(onPerformSave)}
        onDontSaveClick={() => this.interpreter.send(onPerformNoSave)}
        onCancelClick={() => this.interpreter.send(onPerformCancel)}
      />
    );
  }

  @action.bound
  run(): void {
    this.interpreter.start();
  }

  @computed get isDirtySession() {
    return getFormScreen(this).isDirty;
  }

  get isReadData() {
    return !getDontRequestData(this);
  }

  parent?: any;
}
