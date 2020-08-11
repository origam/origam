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
import { IDataView } from "../types/IDataView";
import { IAggregationInfo } from "../types/IAggregationInfo";
import { SCROLL_ROW_CHUNK } from "../../../gui/Workbench/ScreenArea/TableView/InfiniteScrollLoader";
import { IQueryInfo, processActionQueryInfo } from "model/actions/Actions/processActionQueryInfo";
import { assignIIds } from "xmlInterpreters/xmlUtils";
import { IOrderByDirection, IOrdering } from "../types/IOrderingConfiguration";
import { getOrderingConfiguration } from "../../selectors/DataView/getOrderingConfiguration";
import { getFilterConfiguration } from "../../selectors/DataView/getFilterConfiguration";
import { getUserFilters } from "../../selectors/DataView/getUserFilters";
import { getUserOrdering } from "../../selectors/DataView/getUserOrdering";
import { FlowBusyMonitor } from "../../../utils/flow";
import { IScreenEvents } from "../../../modules/Screen/FormScreen/ScreenEvents";
import { scopeFor } from "../../../dic/Container";

enum IQuestionSaveDataAnswer {
  Cancel = 0,
  NoSave = 1,
  Save = 2,
}

enum IQuestionDeleteDataAnswer {
  No = 0,
  Yes = 1,
}

export class FormScreenLifecycle02 implements IFormScreenLifecycle02 {
  $type_IFormScreenLifecycle: 1 = 1;

  @observable allDataViewsSteady = true;

  monitor: FlowBusyMonitor = new FlowBusyMonitor();

  get isWorking() {
    return this.monitor.isWorking;
  }

  disposers: (() => void)[] = [];

  registerDisposer(disposer: () => void) {
    this.disposers.push(disposer);
  }

  *onFlushData(): Generator<unknown, any, unknown> {
    yield* this.flushData();
  }

  *onCreateRow(entity: string, gridId: string): Generator<unknown, any, unknown> {
    yield* this.createRow(entity, gridId);
  }

  *onCopyRow(entity: any, gridId: string, rowId: string): any {
    yield* this.copyRow(entity, gridId, rowId);
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
  ): Generator<any, any, any> {
    yield* this.executeAction(gridId, entity, action, selectedItems);
  }

  *onRequestDeleteRow(entity: string, rowId: string) {
    if ((yield this.questionDeleteData()) === IQuestionDeleteDataAnswer.Yes) {
      yield* this.deleteRow(entity, rowId);
    }
  }

  *onRequestScreenClose(): Generator<unknown, any, unknown> {
    const formScreen = getFormScreen(this);
    // Just wait if there is some data manipulation in progress.
    yield formScreen.dataUpdateCRS.runAsync(() => Promise.resolve());
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

  *onWorkflowNextClick(event: any): Generator {
    this.monitor.inFlow++;
    try {
      const api = getApi(this);
      const sessionId = getSessionId(this);
      const actionQueryInfo = (yield api.workflowNextQuery({
        sessionFormIdentifier: sessionId,
      })) as IQueryInfo[];
      const processQueryInfoResult = yield* processActionQueryInfo(this)(actionQueryInfo);
      if (!processQueryInfoResult.canContinue) return;
      let uiResult;
      const formScreen = getFormScreen(this);
      try {
        yield* formScreen.dataUpdateCRS.enterGenerator();
        const uiResult = yield api.workflowNext({
          sessionFormIdentifier: sessionId,
          CachedFormIds: [],
        });
      } finally {
        formScreen.dataUpdateCRS.leave();
      }
      this.killForm();
      yield* this.start(uiResult);
    } finally {
      this.monitor.inFlow--;
    }
  }

  *onWorkflowAbortClick(event: any): Generator {
    this.monitor.inFlow++;
    try {
      const api = getApi(this);
      let uiResult;
      const formScreen = getFormScreen(this);
      try {
        yield* formScreen.dataUpdateCRS.enterGenerator();
        uiResult = yield api.workflowAbort({ sessionFormIdentifier: getSessionId(this) });
      } finally {
        formScreen.dataUpdateCRS.leave();
      }
      this.killForm();
      yield* this.start(uiResult);
    } finally {
      this.monitor.inFlow--;
    }
  }

  *onWorkflowRepeatClick(event: any): Generator {
    this.monitor.inFlow++;
    try {
      const api = getApi(this);
      const sessionId = getSessionId(this);
      let uiResult;
      const formScreen = getFormScreen(this);
      try {
        yield* formScreen.dataUpdateCRS.enterGenerator();
        uiResult = yield api.workflowRepeat({ sessionFormIdentifier: sessionId });
      } finally {
        formScreen.dataUpdateCRS.leave();
      }
      this.killForm();
      yield* this.start(uiResult);
    } finally {
      this.monitor.inFlow--;
    }
  }

  *onWorkflowCloseClick(event: any): Generator {
    this.monitor.inFlow++;
    try {
      const formScreen = getFormScreen(this);
      try {
        yield* formScreen.dataUpdateCRS.enterGenerator();
      } finally {
        formScreen.dataUpdateCRS.leave();
      }
      yield* this.onRequestScreenClose();
    } finally {
      this.monitor.inFlow--;
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
    flow(function* () {
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
    this.disposers.push(
      reaction(
        () => getFormScreen(this).dataViews.every((dv) => !dv.isWorking) && !this.isWorking,
        (allDataViewsSteady) => {
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
      ),
      () => {
        clearTimeout(_steadyDebounceTimeout);
        _steadyDebounceTimeout = undefined;
      }
    );
    // yield* this.initUI();
    yield* this.applyInitUIResult({ initUIResult });
    if (!this.isReadData) {
      yield* this.loadData();
      const formScreen = getFormScreen(this);
      for (let rootDataView of formScreen.rootDataViews) {
        const orderingConfiguration = getOrderingConfiguration(rootDataView);
        const filterConfiguration = getFilterConfiguration(rootDataView);
        this.disposers.push(
          reaction(
            () => {
              orderingConfiguration.ordering.map((x) => x.direction);
              filterConfiguration.filters.map((x) => [
                x.propertyId,
                x.setting.type,
                x.setting.val1,
              ]);
              return [];
            },
            flow(() => this.readFirstChunkOfRows(rootDataView))
          )
        );
      }
    }
    yield* this.startAutorefreshIfNeeded();
  }

  *applyInitUIResult(args: { initUIResult: any }) {
    const openedScreen = getOpenedScreen(this);

    assignIIds(args.initUIResult.formDefinition);

    const screen = interpretScreenXml(
      args.initUIResult.formDefinition,
      this,
      args.initUIResult.panelConfigurations,
      args.initUIResult.lookupMenuMappings,
      args.initUIResult.sessionId
    );
    openedScreen.content.setFormScreen(screen);
    screen.printMasterDetailTree();
    yield* this.applyData(args.initUIResult.data);
    getDataViewList(this).forEach((dv) => dv.start());

    setTimeout(() => {
      const fieldToSelect = getFormScreen(this).getFirstFormPropertyId();
      if (fieldToSelect) {
        const formScreen = getFormScreen(this);
        const $formScreen = scopeFor(formScreen);
        $formScreen?.resolve(IScreenEvents).focusField.trigger({ propertyId: fieldToSelect });
      }
    }, 100);
  }

  loadChildRows(rootDataView: IDataView, filter: string, ordering: IOrdering | undefined) {
    const api = getApi(this);
    return api.getRows({
      MenuId: getMenuItemId(rootDataView),
      SessionFormIdentifier: getSessionId(this),
      DataStructureEntityId: getDataStructureEntityId(rootDataView),
      Filter: filter,
      Ordering: ordering ? [ordering] : [],
      RowLimit: SCROLL_ROW_CHUNK,
      RowOffset: 0,
      ColumnNames: getColumnNamesToLoad(rootDataView),
      MasterRowId: undefined,
    });
  }

  loadChildGroups(
    rootDataView: IDataView,
    filter: string,
    groupByColumn: string,
    aggregations: IAggregationInfo[] | undefined,
    lookupId: string | undefined
  ) {
    const ordering = {
      columnId: groupByColumn,
      direction: IOrderByDirection.ASC,
      lookupId: lookupId,
    };
    const api = getApi(this);
    return api.getGroups({
      MenuId: getMenuItemId(rootDataView),
      SessionFormIdentifier: getSessionId(this),
      DataStructureEntityId: getDataStructureEntityId(rootDataView),
      Filter: filter,
      Ordering: [ordering],
      RowLimit: 999999,
      GroupBy: groupByColumn,
      GroupByLookupId: lookupId,
      MasterRowId: undefined,
      AggregatedColumns: aggregations,
    });
  }

  loadGroups(
    rootDataView: IDataView,
    groupBy: string,
    groupByLookupId: string | undefined,
    aggregations: IAggregationInfo[] | undefined
  ) {
    const api = getApi(this);
    const ordering = {
      columnId: groupBy,
      direction: IOrderByDirection.ASC,
      lookupId: groupByLookupId,
    };

    return api.getGroups({
      MenuId: getMenuItemId(rootDataView),
      SessionFormIdentifier: getSessionId(this),
      DataStructureEntityId: getDataStructureEntityId(rootDataView),
      Filter: "",
      Ordering: [ordering],
      RowLimit: 999999,
      GroupBy: groupBy,
      GroupByLookupId: groupByLookupId,
      MasterRowId: undefined,
      AggregatedColumns: aggregations,
    });
  }

  loadAggregations(rootDataView: IDataView, aggregations: IAggregationInfo[]) {
    const api = getApi(this);
    return api.getAggregations({
      MenuId: getMenuItemId(rootDataView),
      SessionFormIdentifier: getSessionId(this),
      DataStructureEntityId: getDataStructureEntityId(rootDataView),
      Filter: "",
      MasterRowId: undefined,
      AggregatedColumns: aggregations,
    });
  }

  *loadData() {
    const formScreen = getFormScreen(this);
    try {
      this.monitor.inFlow++;
      for (let dataView of formScreen.nonRootDataViews) {
        dataView.dataTable.clear();
        dataView.setSelectedRowId(undefined);
        dataView.lifecycle.stopSelectedRowReaction();
      }
      for (let rootDataView of formScreen.rootDataViews) {
        rootDataView.saveViewState();
        yield* this.readFirstChunkOfRows(rootDataView);
      }
    } finally {
      for (let dataView of formScreen.nonRootDataViews) {
        dataView.lifecycle.startSelectedRowReaction();
      }
      this.monitor.inFlow--;
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
      this.monitor.inFlow++;
      const api = getApi(this);
      do {
        this._flushDataShallRerun = false;
        const formScreen = getFormScreen(this);
        const dataViews = formScreen.dataViews;
        for (let dataView of dataViews) {
          for (let row of dataView.dataTable.getDirtyValueRows()) {
            const self = this;
            const updateObjectResult = yield* formScreen.dataUpdateCRS.runGenerator<any>(
              function* () {
                return yield api.updateObject({
                  SessionFormIdentifier: getSessionId(self),
                  Entity: dataView.entity,
                  Id: dataView.dataTable.getRowId(row),
                  Values: map2obj(dataView.dataTable.getDirtyValues(row)),
                });
              }
            );

            yield* refreshWorkQueues(this)();
            yield* processCRUDResult(dataView, updateObjectResult);
          }
        }
        if (formScreen.requestSaveAfterUpdate) {
          yield* this.saveSession();
        }
      } while (this._flushDataShallRerun);
    } finally {
      this._flushDataRunning = false;
      this.monitor.inFlow--;
    }
  }

  *updateRadioButtonValue(dataView: IDataView, row: any, fieldName: string, newValue: string) {
    try {
      this.monitor.inFlow++;
      const api = getApi(this);
      const changes: any = {};
      changes[fieldName] = newValue;
      const formScreen = getFormScreen(this);
      const self = this;
      const updateObjectResult = yield* formScreen.dataUpdateCRS.runGenerator<any>(function* () {
        return yield api.updateObject({
          SessionFormIdentifier: getSessionId(self),
          Entity: dataView.entity,
          Id: dataView.dataTable.getRowId(row),
          Values: changes,
        });
      });

      yield* processCRUDResult(dataView, updateObjectResult);

      if (formScreen.requestSaveAfterUpdate) {
        yield* this.saveSession();
      }
    } finally {
      this.monitor.inFlow--;
    }
  }

  *readFirstChunkOfRows(rootDataView: IDataView) {
    const api = getApi(this);
    rootDataView.setSelectedRowId(undefined);
    rootDataView.lifecycle.stopSelectedRowReaction();
    try {
      const loadedData = yield api.getRows({
        MenuId: getMenuItemId(rootDataView),
        SessionFormIdentifier: getSessionId(this),
        DataStructureEntityId: getDataStructureEntityId(rootDataView),
        Filter: getUserFilters(rootDataView),
        Ordering: getUserOrdering(rootDataView),
        RowLimit: SCROLL_ROW_CHUNK,
        RowOffset: 0,
        ColumnNames: getColumnNamesToLoad(rootDataView),
        MasterRowId: undefined,
      });
      rootDataView.dataTable.setRecords(loadedData);
      rootDataView.reselectOrSelectFirst();

      //debugger
      rootDataView.restoreViewState();
    } finally {
      rootDataView.lifecycle.startSelectedRowReaction(true);
    }
  }

  *createRow(entity: string, gridId: string) {
    try {
      this.monitor.inFlow++;
      const api = getApi(this);
      const targetDataView = getDataViewByGridId(this, gridId)!;
      const formScreen = getFormScreen(this);
      let createObjectResult;
      try {
        yield* formScreen.dataUpdateCRS.enterGenerator();
        createObjectResult = yield api.createObject({
          SessionFormIdentifier: getSessionId(this),
          Entity: entity,
          RequestingGridId: gridId,
          Values: {},
          Parameters: { ...getBindingParametersFromParent(targetDataView) },
        });
      } finally {
        formScreen.dataUpdateCRS.leave();
      }
      yield* refreshWorkQueues(this)();
      yield* processCRUDResult(targetDataView, createObjectResult);
    } finally {
      this.monitor.inFlow--;
    }
  }

  *copyRow(entity: string, gridId: string, rowId: string) {
    try {
      this.monitor.inFlow++;
      const api = getApi(this);
      const targetDataView = getDataViewByGridId(this, gridId)!;
      const formScreen = getFormScreen(this);
      let createObjectResult;
      try {
        yield* formScreen.dataUpdateCRS.enterGenerator();
        createObjectResult = yield api.copyObject({
          SessionFormIdentifier: getSessionId(this),
          Entity: entity,
          OriginalId: rowId,
          RequestingGridId: gridId,
          Entities: [entity],
          ForcedValues: {},
        });
      } finally {
        formScreen.dataUpdateCRS.leave();
      }
      yield* refreshWorkQueues(this)();
      yield* processCRUDResult(targetDataView, createObjectResult);
    } finally {
      this.monitor.inFlow--;
    }
  }

  *deleteRow(entity: string, rowId: string) {
    try {
      this.monitor.inFlow++;
      const api = getApi(this);
      const formScreen = getFormScreen(this);
      let deleteObjectResult;
      try {
        yield* formScreen.dataUpdateCRS.enterGenerator();
        deleteObjectResult = yield api.deleteObject({
          SessionFormIdentifier: getSessionId(this),
          Entity: entity,
          Id: rowId,
        });
      } finally {
        formScreen.dataUpdateCRS.leave();
      }
      yield* refreshWorkQueues(this)();
      yield* processCRUDResult(this, deleteObjectResult);
    } finally {
      this.monitor.inFlow--;
    }
  }

  *saveSession() {
    if (getIsSuppressSave(this)) {
      return;
    }
    try {
      this.monitor.inFlow++;
      const api = getApi(this);
      let result;
      const formScreen = getFormScreen(this);
      try {
        yield* formScreen.dataUpdateCRS.enterGenerator();
        const queryResult = yield api.saveSessionQuery(getSessionId(this));
        const processQueryInfoResult = yield* processActionQueryInfo(this)(queryResult);
        if (!processQueryInfoResult.canContinue) return;
        result = yield api.saveSession(getSessionId(this));
      } finally {
        formScreen.dataUpdateCRS.leave();
      }
      yield* refreshWorkQueues(this)();
      yield* processCRUDResult(this, result);
    } finally {
      this.monitor.inFlow--;
    }
  }

  *refreshSession() {
    // TODO: Refresh lookups and rowstates !!!
    try {
      this.monitor.inFlow++;
      if (this.isReadData) {
        const formScreen = getFormScreen(this);
        formScreen.dataViews.forEach((dv) => dv.saveViewState());
        const api = getApi(this);
        let result;
        try {
          yield* formScreen.dataUpdateCRS.enterGenerator();
          result = yield api.refreshSession(getSessionId(this));
        } finally {
          formScreen.dataUpdateCRS.leave();
        }
        yield* this.applyData(result);
        getFormScreen(this).setDirty(false);
        getFormScreen(this).dataViews.forEach((dv) => dv.restoreViewState());
      } else {
        yield* this.loadData();
      }
    } finally {
      this.monitor.inFlow--;
      setTimeout(async () => {
        await when(() => this.allDataViewsSteady);
      }, 10);
    }
    yield* clearRowStates(this)();
    yield* refreshWorkQueues(this)();
  }

  *executeAction(gridId: string, entity: string, action: IAction, selectedItems: string[]) {
    try {
      this.monitor.inFlow++;
      const parameters: { [key: string]: any } = {};
      for (let parameter of action.parameters) {
        parameters[parameter.name] = parameter.fieldName;
      }
      const api = getApi(this);
      const formScreen = getFormScreen(this);
      let result;
      try {
        yield* formScreen.dataUpdateCRS.enterGenerator();
        const queryResult = (yield api.executeActionQuery({
          SessionFormIdentifier: getSessionId(this),
          Entity: entity,
          ActionType: action.type,
          ActionId: action.id,
          ParameterMappings: parameters,
          SelectedItems: selectedItems,
          InputParameters: {},
        })) as IQueryInfo[];
        const processQueryInfoResult = yield* processActionQueryInfo(this)(queryResult);
        if (!processQueryInfoResult.canContinue) return;

        const self = this;
        result = yield api.executeAction({
          SessionFormIdentifier: getSessionId(self),
          Entity: entity,
          ActionType: action.type,
          ActionId: action.id,
          ParameterMappings: parameters,
          SelectedItems: selectedItems,
          InputParameters: {},
          RequestingGrid: gridId,
        });
      } finally {
        formScreen.dataUpdateCRS.leave();
      }

      yield* refreshWorkQueues(this)();
      yield* new_ProcessActionResult(action)(result);
    } finally {
      this.monitor.inFlow--;
    }
  }

  @action.bound
  killForm() {
    this.clearAutorefreshInterval();
    this.disposers.forEach((disposer) => disposer());
    getDataViewList(this).forEach((dv) => dv.stop());
    const openedScreen = getOpenedScreen(this);
    openedScreen.content.setFormScreen(undefined);
  }

  *closeForm() {
    try {
      this.monitor.inFlow++;
      yield* closeForm(this)();
      this.killForm();
    } finally {
      this.monitor.inFlow--;
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

  *applyData(data: any): Generator {
    for (let [entityKey, entityValue] of Object.entries(data || {})) {
      const dataViews = getDataViewsByEntity(this, entityKey);
      for (let dataView of dataViews) {
        dataView.dataTable.setRecords((entityValue as any).data);
        dataView.reselectOrSelectFirst();
      }
    }
  }

  get isReadData() {
    return !getOpenedScreen(this).dontRequestData;
  }

  parent?: any;
}
