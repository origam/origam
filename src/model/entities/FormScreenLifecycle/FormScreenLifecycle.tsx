import { QuestionDeleteData } from "gui/Components/Dialogs/QuestionDeleteData";
import { QuestionSaveData } from "gui/Components/Dialogs/QuestionSaveData";
import { action, autorun, computed, flow, observable, reaction, when } from "mobx";
import { new_ProcessActionResult } from "model/actions/Actions/processActionResult";
import { closeForm } from "model/actions/closeForm";
import { processCRUDResult } from "model/actions/DataLoading/processCRUDResult";
import { handleError } from "model/actions/handleError";
import { clearRowStates } from "model/actions/RowStates/clearRowStates";
import { refreshWorkQueues } from "model/actions/WorkQueues/refreshWorkQueues";
import { IAction } from "model/entities/types/IAction";
import { getBindingParametersFromParent } from "model/selectors/DataView/getBindingParametersFromParent";
import { getColumnNamesToLoad } from "model/selectors/DataView/getColumnNamesToLoad";
import { getDataStructureEntityId } from "model/selectors/DataView/getDataStructureEntityId";
import { getDataViewByGridId } from "model/selectors/DataView/getDataViewByGridId";
import { getDataViewsByEntity } from "model/selectors/DataView/getDataViewsByEntity";
import { getAutorefreshPeriod } from "model/selectors/FormScreen/getAutorefreshPeriod";
import { getDataViewList } from "model/selectors/FormScreen/getDataViewList";
import { getIsFormScreenDirty } from "model/selectors/FormScreen/getisFormScreenDirty";
import { getIsSuppressSave } from "model/selectors/FormScreen/getIsSuppressSave";
import { getDialogStack } from "model/selectors/getDialogStack";
import { getIsActiveScreen } from "model/selectors/getIsActiveScreen";
import React from "react";
import { map2obj } from "utils/objects";
import { interpretScreenXml } from "xmlInterpreters/screenXml";
import { getFormScreen } from "../../selectors/FormScreen/getFormScreen";
import { getApi } from "../../selectors/getApi";
import { getMenuItemId } from "../../selectors/getMenuItemId";
import { getOpenedScreen } from "../../selectors/getOpenedScreen";
import { getSessionId } from "../../selectors/getSessionId";
import { IFormScreenLifecycle02 } from "../types/IFormScreenLifecycle";
import { getGroupingConfiguration } from "../../selectors/TablePanelView/getGroupingConfiguration";
import { IDataView } from "../types/IDataView";

enum IQuestionSaveDataAnswer {
  Cancel = 0,
  NoSave = 1,
  Save = 2
}

enum IQuestionDeleteDataAnswer {
  No = 0,
  Yes = 1
}

export class FormScreenLifecycle02 implements IFormScreenLifecycle02 {
  $type_IFormScreenLifecycle: 1 = 1;

  constructor() {}

  @observable allDataViewsSteady = true;

  @computed get isWorking() {
    return this.inFlow > 0;
  }
  @observable inFlow = 0;
  disposers: any[] = [];

  *onFlushData(): Generator<unknown, any, unknown> {
    yield* this.flushData();
  }

  *onCreateRow(entity: string, gridId: string): Generator<unknown, any, unknown> {
    yield* this.createRow(entity, gridId);
  }

  *onDeleteRow(entity: string, rowId: string): Generator<unknown, any, unknown> {
    yield* this.onRequestDeleteRow(entity, rowId);
  }

  *onSaveSession(): Generator<unknown, any, unknown> {
    yield* this.saveSession();
  }

  *onExecuteAction(
    gridId: string,
    entity: string,
    action: IAction,
    selectedItems: string[]
  ): Generator<unknown, any, unknown> {
    yield* this.executeAction(gridId, entity, action, selectedItems);
  }

  *onRequestDeleteRow(entity: string, rowId: string) {
    if ((yield this.questionDeleteData()) === IQuestionDeleteDataAnswer.Yes) {
      yield* this.deleteRow(entity, rowId);
    }
  }

  *onRequestScreenClose(): Generator<unknown, any, unknown> {
    if (!getIsFormScreenDirty(this) || getIsSuppressSave(this)) {
      yield* this.closeForm();
      return;
    }
    switch (yield this.questionSaveData()) {
      case IQuestionSaveDataAnswer.Cancel:
        return;
      case IQuestionSaveDataAnswer.Save:
        yield* this.saveSession();
        yield* this.closeForm();
        return;
      case IQuestionSaveDataAnswer.NoSave:
        yield* this.closeForm();
        return;
    }
  }

  *onRequestScreenReload(): Generator<unknown, any, unknown> {
    if (!getIsFormScreenDirty(this) || getIsSuppressSave(this)) {
      yield* this.refreshSession();
      return;
    }
    switch (yield this.questionSaveData()) {
      case IQuestionSaveDataAnswer.Cancel:
        return;
      case IQuestionSaveDataAnswer.Save:
        yield* this.saveSession();
        yield* this.refreshSession();
        return;
      case IQuestionSaveDataAnswer.NoSave:
        yield* this.refreshSession();
        return;
    }
  }

  _autorefreshTimerHandle: any;

  *startAutorefreshIfNeeded() {
    const autorefreshPeriod = getAutorefreshPeriod(this);
    if (autorefreshPeriod) {
      this.disposers.push(
        autorun(() => {
          if (
            !getIsSuppressSave(this) &&
            (getIsFormScreenDirty(this) || !getIsActiveScreen(this))
          ) {
            this.clearAutorefreshInterval();
          } else {
            this._autorefreshTimerHandle = setInterval(
              () => this.performAutoreload(),
              autorefreshPeriod * 1000
            );
          }
        })
      );
    }
  }

  clearAutorefreshInterval() {
    if (this._autorefreshTimerHandle) {
      clearInterval(this._autorefreshTimerHandle);
      this._autorefreshTimerHandle = undefined;
    }
  }

  performAutoreload() {
    const self = this;
    flow(function*() {
      try {
        yield* self.refreshSession();
      } catch (e) {
        yield* handleError(self)(e);
        throw e;
      }
    })();
  }

  *start(initUIResult: any): Generator {
    let _steadyDebounceTimeout: any;
    reaction(
      () => getFormScreen(this).dataViews.every(dv => !dv.isWorking) && !this.isWorking,
      allDataViewsSteady => {
        if (allDataViewsSteady) {
          _steadyDebounceTimeout = setTimeout(() => {
            _steadyDebounceTimeout = undefined;
            this.allDataViewsSteady = true;
          }, 100);
        } else {
          this.allDataViewsSteady = false;
          if (_steadyDebounceTimeout) {
            clearTimeout(_steadyDebounceTimeout);
            _steadyDebounceTimeout = undefined;
          }
        }
      }
    );
    autorun(() => {
      console.log("ALL DATA VIEWS STEADY:", this.allDataViewsSteady);
    });
    // yield* this.initUI();
    yield* this.applyInitUIResult({ initUIResult });
    if (!this.isReadData) {
      yield* this.loadData(true);
    }
    yield* this.startAutorefreshIfNeeded();
  }

  /*
  *initUI() {
    try {
      this.inFlow++;
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
      yield* this.applyInitUIResult({ initUIResult });
    } catch (error) {
      yield* handleError(this)(error);
      yield* closeForm(this)();
      throw error;
      // TODO: Error handling !
    } finally {
      this.inFlow--;
    }
  }*/



  *applyInitUIResult(args: { initUIResult: any }) {
    const openedScreen = getOpenedScreen(this);

    const screen = interpretScreenXml(
      args.initUIResult.formDefinition,
      this,
      args.initUIResult.panelConfigurations,
      args.initUIResult.lookupMenuMappings,
      args.initUIResult.sessionId
    );
    openedScreen.content.setFormScreen(screen);
    screen.printMasterDetailTree();
    yield* this.applyData(args.initUIResult.data, true);
    getDataViewList(this).forEach(dv => dv.start());
  }

  loadChildRows(rootDataView: IDataView, filter: string){
    const api = getApi(this);
    return api.getRows({
      MenuId: getMenuItemId(rootDataView),
      SessionFormIdentifier: getSessionId(this),
      DataStructureEntityId: getDataStructureEntityId(rootDataView),
      Filter: filter,
      Ordering: [],
      RowLimit: 999999,
      ColumnNames: getColumnNamesToLoad(rootDataView),
      MasterRowId: undefined
    });
  }

  loadChildGroups(rootDataView: IDataView, filter: string, groupByColumn: string){
    const api = getApi(this);
    return api.getGroups({
      MenuId: getMenuItemId(rootDataView),
      SessionFormIdentifier: getSessionId(this),
      DataStructureEntityId: getDataStructureEntityId(rootDataView),
      Filter: filter,
      Ordering: [],
      RowLimit: 999999,
      GroupBy: groupByColumn,
      GroupByLookupId: undefined,
      MasterRowId: undefined,
      AggregatedColumn: undefined
    })
  }

  loadGroups(rootDataView: IDataView, groupBy: string, groupByLookupId: string | undefined){
    const api = getApi(this);
    return api.getGroups({
      MenuId: getMenuItemId(rootDataView),
      SessionFormIdentifier: getSessionId(this),
      DataStructureEntityId: getDataStructureEntityId(rootDataView),
      Filter: "",
      Ordering: [],
      RowLimit: 999999,
      GroupBy: groupBy,
      GroupByLookupId: groupByLookupId,
      MasterRowId: undefined,
      AggregatedColumn: undefined
    });
  }

  *loadData(selectFirstRow: boolean) {
    const api = getApi(this);
    const formScreen = getFormScreen(this);
    try {
      this.inFlow++;
      for (let dataView of formScreen.nonRootDataViews) {
        dataView.dataTable.clear();
        dataView.setSelectedRowId(undefined);
        dataView.lifecycle.stopSelectedRowReaction();
      }
      for (let rootDataView of formScreen.rootDataViews) {
        rootDataView.saveViewState();
        rootDataView.dataTable.clear();
        rootDataView.setSelectedRowId(undefined);
        rootDataView.lifecycle.stopSelectedRowReaction();
        try {
          const loadedData = yield api.getRows({
            MenuId: getMenuItemId(rootDataView),
            SessionFormIdentifier: getSessionId(this),
            DataStructureEntityId: getDataStructureEntityId(rootDataView),
            Filter: "",
            Ordering: [],
            RowLimit: 999999,
            ColumnNames: getColumnNamesToLoad(rootDataView),
            MasterRowId: undefined
          });
          rootDataView.dataTable.setRecords(loadedData);
          if (selectFirstRow) {
            rootDataView.selectFirstRow();
          }
          //debugger
          rootDataView.restoreViewState();
        } finally {
          rootDataView.lifecycle.startSelectedRowReaction(true);
        }
      }
    } finally {
      for (let dataView of formScreen.nonRootDataViews) {
        dataView.lifecycle.startSelectedRowReaction();
      }
      this.inFlow--;
    }
  }

  _flushDataRunning = false;
  _flushDataShallRerun = false;
  *flushData() {
    try {
      if (this._flushDataRunning) {
        this._flushDataShallRerun = true;
        return;
      }
      this._flushDataRunning = true;
      this.inFlow++;
      const api = getApi(this);
      do {
        this._flushDataShallRerun = false;
        for (let dataView of getFormScreen(this).dataViews) {
          for (let row of dataView.dataTable.getDirtyValueRows()) {
            const updateObjectResult = yield api.updateObject({
              SessionFormIdentifier: getSessionId(this),
              Entity: dataView.entity,
              Id: dataView.dataTable.getRowId(row),
              Values: map2obj(dataView.dataTable.getDirtyValues(row))
            });
            yield* refreshWorkQueues(this)();
            yield* processCRUDResult(dataView, updateObjectResult);
          }
        }
      } while (this._flushDataShallRerun);
    } finally {
      this._flushDataRunning = false;
      this.inFlow--;
    }
  }

  *createRow(entity: string, gridId: string) {
    try {
      this.inFlow++;
      const api = getApi(this);
      const targetDataView = getDataViewByGridId(this, gridId)!;
      const createObjectResult = yield api.createObject({
        SessionFormIdentifier: getSessionId(this),
        Entity: entity,
        RequestingGridId: gridId,
        Values: {},
        Parameters: { ...getBindingParametersFromParent(targetDataView) }
      });
      yield* refreshWorkQueues(this)();
      yield* processCRUDResult(targetDataView, createObjectResult);
    } finally {
      this.inFlow--;
    }
  }

  *deleteRow(entity: string, rowId: string) {
    try {
      this.inFlow++;
      const api = getApi(this);
      const deleteObjectResult = yield api.deleteObject({
        SessionFormIdentifier: getSessionId(this),
        Entity: entity,
        Id: rowId
      });
      console.log(deleteObjectResult);
      yield* refreshWorkQueues(this)();
      yield* processCRUDResult(this, deleteObjectResult);
    } finally {
      this.inFlow--;
    }
  }

  *saveSession() {
    if (getIsSuppressSave(this)) {
      return;
    }
    try {
      this.inFlow++;
      const api = getApi(this);
      yield api.saveSessionQuery(getSessionId(this));
      const result = yield api.saveSession(getSessionId(this));
      yield* refreshWorkQueues(this)();
      yield* processCRUDResult(this, result);
    } finally {
      this.inFlow--;
    }
  }

  *refreshSession() {
    // TODO: Refresh lookups and rowstates !!!
    try {
      this.inFlow++;
      if (this.isReadData) {
        getFormScreen(this).dataViews.forEach(dv => dv.saveViewState());
        const api = getApi(this);
        const result = yield api.refreshSession(getSessionId(this));
        yield* this.applyData(result, false);
        getFormScreen(this).setDirty(false);
        getFormScreen(this).dataViews.forEach(dv => dv.restoreViewState());
      } else {
        yield* this.loadData(false);
      }
    } finally {
      this.inFlow--;
      setTimeout(async () => {
        console.log(getFormScreen(this).dataViews.map(dv => !dv.isWorking));
        await when(() => this.allDataViewsSteady);
        console.log("Refreshing view state.");
      }, 10);
    }
    yield* clearRowStates(this)();
    yield* refreshWorkQueues(this)();
  }

  *executeAction(gridId: string, entity: string, action: IAction, selectedItems: string[]) {
    try {
      this.inFlow++;
      const parameters: { [key: string]: any } = {};
      for (let parameter of action.parameters) {
        parameters[parameter.name] = parameter.fieldName;
      }
      const api = getApi(this);
      const queryResult = yield api.executeActionQuery({
        SessionFormIdentifier: getSessionId(this),
        Entity: entity,
        ActionType: action.type,
        ActionId: action.id,
        ParameterMappings: parameters,
        SelectedItems: selectedItems,
        InputParameters: {}
      });
      console.log("EAQ", queryResult);

      const result = yield api.executeAction({
        SessionFormIdentifier: getSessionId(this),
        Entity: entity,
        ActionType: action.type,
        ActionId: action.id,
        ParameterMappings: parameters,
        SelectedItems: selectedItems,
        InputParameters: {},
        RequestingGrid: gridId
      });
      console.log("EA", result);
      yield* refreshWorkQueues(this)();
      yield* new_ProcessActionResult(action)(result);
    } finally {
      this.inFlow--;
    }
  }

  *closeForm() {
    try {
      this.inFlow++;
      this.clearAutorefreshInterval();
      this.disposers.forEach(disposer => disposer());
      yield* closeForm(this)();
    } finally {
      this.inFlow--;
    }
  }

  questionSaveData() {
    return new Promise(
      action((resolve: (value: IQuestionSaveDataAnswer) => void) => {
        const closeDialog = getDialogStack(this).pushDialog(
          "",
          <QuestionSaveData
            screenTitle={getOpenedScreen(this).title}
            onSaveClick={() => {
              closeDialog();
              resolve(IQuestionSaveDataAnswer.Save);
            }}
            onDontSaveClick={() => {
              closeDialog();
              resolve(IQuestionSaveDataAnswer.NoSave);
            }}
            onCancelClick={() => {
              closeDialog();
              resolve(IQuestionSaveDataAnswer.Cancel);
            }}
          />
        );
      })
    );
  }

  questionDeleteData() {
    return new Promise(
      action((resolve: (value: IQuestionDeleteDataAnswer) => void) => {
        const closeDialog = getDialogStack(this).pushDialog(
          "",
          <QuestionDeleteData
            screenTitle={getOpenedScreen(this).title}
            onNoClick={() => {
              closeDialog();
              resolve(IQuestionDeleteDataAnswer.No);
            }}
            onYesClick={() => {
              closeDialog();
              resolve(IQuestionDeleteDataAnswer.Yes);
            }}
          />
        );
      })
    );
  }

  *applyData(data: any, selectFirstRow: boolean): Generator {
    for (let [entityKey, entityValue] of Object.entries(data || {})) {
      console.log(entityKey, entityValue);
      const dataViews = getDataViewsByEntity(this, entityKey);
      for (let dataView of dataViews) {
        dataView.dataTable.clear();
        dataView.dataTable.setRecords((entityValue as any).data);
        if (selectFirstRow) {
          dataView.selectFirstRow();
        }
      }
    }
  }

  get isReadData() {
    return !getOpenedScreen(this).dontRequestData;
  }

  parent?: any;
}
