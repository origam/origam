import { QuestionDeleteData } from "gui/Components/Dialogs/QuestionDeleteData";
import { QuestionSaveData } from "gui/Components/Dialogs/QuestionSaveData";
import { action, autorun, flow, observable, reaction, when } from "mobx";
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
import { getUserFilterLookups } from "../../selectors/DataView/getUserFilterLookups";
import _ from "lodash";
import { ChangeMasterRecordDialog } from "../../../gui/Components/Dialogs/ChangeMasterRecordDialog";
import { getFormScreenLifecycle } from "../../selectors/FormScreen/getFormScreenLifecycle";
import { selectFirstRow } from "../../actions/DataView/selectFirstRow";
import { YesNoQuestion } from "gui/Components/Dialogs/YesNoQuestion";
import { getProperties } from "model/selectors/DataView/getProperties";
import { getWorkbench } from "model/selectors/getWorkbench";
import { shouldProceedToChangeRow } from "model/actions-ui/DataView/TableView/shouldProceedToChangeRow";
import { getGroupingConfiguration } from "model/selectors/TablePanelView/getGroupingConfiguration";
import {IDataViewToolbarUI} from "modules/DataView/DataViewUI";
import {IFormPerspectiveDirector} from "modules/DataView/Perspective/FormPerspective/FormPerspectiveDirector";

enum IQuestionSaveDataAnswer {
  Cancel = 0,
  NoSave = 1,
  Save = 2,
}

enum IQuestionDeleteDataAnswer {
  No = 0,
  Yes = 1,
}

enum IQuestionChangeRecordAnswer {
  Yes = 0,
  No = 1,
  Cancel = 2,
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
    if (action.confirmationMessage && !(yield this.askYesNoQuestion(action.confirmationMessage))) {
      return;
    }
    yield* this.executeAction(gridId, entity, action, selectedItems);
  }

  askYesNoQuestion(question: string) {
    return new Promise(
      action((resolve: (value: boolean) => void) => {
        const closeDialog = getDialogStack(this).pushDialog(
          "",
          <YesNoQuestion
            screenTitle={getOpenedScreen(this).title}
            message={question}
            onYesClick={() => {
              closeDialog();
              resolve(true);
            }}
            onNoClick={() => {
              closeDialog();
              resolve(false);
            }}
          />
        );
      })
    );
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
    getFormScreen(this).dataViews.forEach((dataView) => dataView.onReload());
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
      const formScreen = getFormScreen(this);
      let uiResult;
      try {
        yield* formScreen.dataUpdateCRS.enterGenerator();
        uiResult = yield api.workflowNext({
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
              orderingConfiguration.userOrderings.map((x) => x.direction);
              filterConfiguration.activeFilters.map((x) => [
                x.propertyId,
                x.setting.type,
                x.setting.val1,
              ]);
              return [] as any;
            },
            () => this.sortAndFilterReaction(rootDataView)
          )
        );
      }
    }
    yield* this.startAutorefreshIfNeeded();
  }

  sortAndFilterReaction(dataView : IDataView){
    const self=this;
    flow(function* () {
      if (!(yield shouldProceedToChangeRow(dataView))) {
        return;
      }
      yield dataView.lifecycle.runRecordChangedReaction(function*() {
        self.readFirstChunkOfRowsWithGateDebounced(dataView);
      });
    })();
  }

  *applyInitUIResult(args: { initUIResult: any }) {
    const openedScreen = getOpenedScreen(this);

    assignIIds(args.initUIResult.formDefinition);

    const { formScreen: screen, foundLookupIds } = yield* interpretScreenXml(
      args.initUIResult.formDefinition,
      this,
      args.initUIResult.panelConfigurations,
      args.initUIResult.lookupMenuMappings,
      args.initUIResult.sessionId
    );
    const api = getApi(openedScreen);
    const cacheDependencies = getWorkbench(openedScreen).lookupMultiEngine.cacheDependencies;
    const lookupIdsToQuery = cacheDependencies.getUnhandledLookupIds(foundLookupIds);

    if (lookupIdsToQuery.size > 0) {
      const dependencies = yield api.getLookupCacheDependencies({
        LookupIds: Array.from(lookupIdsToQuery),
      });
      cacheDependencies.putValues(dependencies);
    }

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
        const groupingConfiguration = getGroupingConfiguration(rootDataView);
        if (groupingConfiguration.isGrouping) {
          rootDataView.serverSideGrouper.refresh();
        } else {
          yield* this.readFirstChunkOfRows(rootDataView);
        }
      }
    } finally {
      for (let dataView of formScreen.nonRootDataViews) {
        dataView.lifecycle.startSelectedRowReaction();
      }
      this.monitor.inFlow--;
    }
  }

  *throwChangesAway(dataView: IDataView) {
    try {
      this.monitor.inFlow++;
      const api = getApi(this);
      const updateObjectResult = yield api.restoreData({
        SessionFormIdentifier: getSessionId(this),
        ObjectId: dataView.selectedRowId!,
      });
      yield* processCRUDResult(dataView, updateObjectResult);
      const formScreen = getFormScreen(this);
      if (formScreen.requestSaveAfterUpdate) {
        yield* this.saveSession();
      }
    } finally {
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
        FilterLookups: getUserFilterLookups(rootDataView),
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

  _readFirstChunkOfRowsRunning = false;
  _readFirstChunkOfRowsScheduled = false;
  *readFirstChunkOfRowsWithGate(rootDataView: IDataView) {
    try {
      if (this._readFirstChunkOfRowsRunning) {
        this._readFirstChunkOfRowsScheduled = true;
        return;
      }
      this._readFirstChunkOfRowsRunning = true;
      do {
        this._readFirstChunkOfRowsScheduled = false;
        yield* this.readFirstChunkOfRows(rootDataView);
      } while (this._readFirstChunkOfRowsScheduled);
    } finally {
      this._readFirstChunkOfRowsRunning = false;
      this._readFirstChunkOfRowsScheduled = false;
    }
  }

  readFirstChunkOfRowsWithGateDebounced = _.debounce(
    flow(this.readFirstChunkOfRowsWithGate.bind(this)),
    500
  );

  private getNewRowValues(){
    if(!this.userOrderedDataView) {
      return {}
    }

    const orderProperty = this.userOrderedDataView.orderProperty!;
    const orderValues = this.userOrderedDataView.tableRows
      .filter(row => Array.isArray)
      .map(row => (row as any[])[orderProperty.dataIndex] as number);
    const nextOrderValue = Math.max(...orderValues) + 1
    const orderPropName = orderProperty.name;
    const values = {} as any;
    values[orderProperty.name] = nextOrderValue;
    return values
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
          Values: this.getNewRowValues(),
          Parameters: { ...getBindingParametersFromParent(targetDataView) },
        });
      } finally {
        formScreen.dataUpdateCRS.leave();
      }
      yield* refreshWorkQueues(this)();
      yield* processCRUDResult(targetDataView, createObjectResult);
      yield* selectFirstRow(targetDataView)();
      if(targetDataView.newRecordView === "0" && targetDataView.activateFormView){
        yield* targetDataView.activateFormView();
      }
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
          ForcedValues: this.getNewRowValues(),
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

  private get userOrderedDataView(){
    return getFormScreen(this).dataViews.find(dataView => dataView.orderProperty);
  }

  *deleteRow(entity: string, rowId: string) {
    try {
      this.monitor.inFlow++;
      const api = getApi(this);
      const formScreen = getFormScreen(this);
      let deleteObjectResult;
      try {
        yield* formScreen.dataUpdateCRS.enterGenerator();

        if(this.userOrderedDataView) {
          deleteObjectResult = yield* this.deleteObjectInOrderedList(rowId, entity, this.userOrderedDataView);
        }else{
          deleteObjectResult = yield api.deleteObject({
            SessionFormIdentifier: getSessionId(this),
            Entity: entity,
            Id: rowId,
          });
        }
      } finally {
        formScreen.dataUpdateCRS.leave();
      }
      yield* refreshWorkQueues(this)();
      yield* processCRUDResult(this, deleteObjectResult);
    } finally {
      this.monitor.inFlow--;
    }
  }

  private *deleteObjectInOrderedList(rowId: string, entity: string, dataView: IDataView) {
    const api = getApi(this);
    const rowToDelete = dataView.dataTable.getRowById(rowId)!;
    const orderProperty = dataView.orderProperty!;
    const newRowOrderMap = {} as any;
    if (orderProperty) {
      dataView.dataTable.allRows
        .filter(row => row[orderProperty.dataIndex] > rowToDelete[orderProperty.dataIndex])
        .forEach(row => {
          const rowId = dataView.dataTable.getRowId(row);
          const newOrder = row[orderProperty.dataIndex] - 1;
          newRowOrderMap[rowId] = newOrder;
        })
    }
    return yield api.deleteObjectInOrderedList({
      SessionFormIdentifier: getSessionId(this),
      Entity: entity,
      Id: rowId,
      OrderProperty: orderProperty.name,
      UpdatedOrderValues: newRowOrderMap
    });
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
        getFormScreen(this).dataViews
          .forEach(dataView => dataView.dataTable.unlockAddedRowPosition());
      } finally {
        formScreen.dataUpdateCRS.leave();
      }
      yield* refreshWorkQueues(this)();
      yield* processCRUDResult(this, result);
      getFormScreen(this).dataViews
        .forEach(dataView => dataView.dataTable.updateSortAndFilter());
    } finally {
      this.monitor.inFlow--;
    }
  }

  *refreshLookups() {
    const dataViews = getDataViewList(this);
    const properties = dataViews.flatMap((dv) => getProperties(dv)).filter((prop) => prop.isLookup);
    const cleaned = new Set<any>();
    for (let prop of properties) {
      if (prop.lookupEngine && !cleaned.has(prop.lookupId)) {
        //console.log("Cleaning and reloading lookup caches:", prop.id, prop.lookupId);
        prop.lookupEngine.cleanAndReload();
        getWorkbench(this).lookupListCache.deleteLookup(prop.lookupId!);
        cleaned.add(prop.lookupId);
      }
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
      yield* this.refreshLookups();
    } finally {
      this.monitor.inFlow--;
      setTimeout(async () => {
        await when(() => this.allDataViewsSteady);
      }, 10);
    }
    yield* clearRowStates(this)();
    yield* refreshWorkQueues(this)();
  }

  loadInitialData(){
    const self = this;
    flow(function*(){
      yield* self.loadData();
    })();
  }

  private actionRunning = false;

  *executeAction(gridId: string, entity: string, action: IAction, selectedItems: string[]) {
    if(this.actionRunning){
      return;
    }
    this.actionRunning = true;
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
      this.actionRunning = false;
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

  async handleUserInputOnChangingRow(dataView: IDataView) {
    const api = getApi(dataView);
    const openedScreen = getOpenedScreen(this);
    const sessionId = getSessionId(openedScreen.content.formScreen);

    switch (await this.questionSaveDataAfterRecordChange()) {
      case IQuestionChangeRecordAnswer.Cancel:
        return false;
      case IQuestionChangeRecordAnswer.Yes:
        await api.saveSessionQuery(sessionId);
        await api.saveSession(sessionId);
        return true;
      case IQuestionChangeRecordAnswer.No:
        await flow(() => getFormScreenLifecycle(dataView).throwChangesAway(dataView))();
        return true;
      default:
        throw new Error("Option not implemented");
    }
  }

  questionSaveDataAfterRecordChange() {
    return new Promise(
      action((resolve: (value: IQuestionChangeRecordAnswer) => void) => {
        const closeDialog = getDialogStack(this).pushDialog(
          "",
          <ChangeMasterRecordDialog
            screenTitle={getOpenedScreen(this).title}
            onSaveClick={() => {
              closeDialog();
              resolve(IQuestionChangeRecordAnswer.Yes);
            }}
            onDontSaveClick={() => {
              closeDialog();
              resolve(IQuestionChangeRecordAnswer.No);
            }}
            onCancelClick={() => {
              closeDialog();
              resolve(IQuestionChangeRecordAnswer.Cancel);
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
