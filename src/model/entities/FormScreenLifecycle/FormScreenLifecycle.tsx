import { QuestionSaveData } from "gui/Components/Dialogs/QuestionSaveData";
import { action, computed, observable } from "mobx";
import { processActionResult } from "model/actions/Actions/processActionResult";
import { closeForm } from "model/actions/closeForm";
import { processCRUDResult } from "model/actions/DataLoading/processCRUDResult";
import { IAction } from "model/entities/types/IAction";
import { getBindingParametersFromParent } from "model/selectors/DataView/getBindingParametersFromParent";
import { getColumnNamesToLoad } from "model/selectors/DataView/getColumnNamesToLoad";
import { getDataStructureEntityId } from "model/selectors/DataView/getDataStructureEntityId";
import { getDataViewByGridId } from "model/selectors/DataView/getDataViewByGridId";
import { getDataViewsByEntity } from "model/selectors/DataView/getDataViewsByEntity";
import { getDataViewList } from "model/selectors/FormScreen/getDataViewList";
import { getDialogStack } from "model/selectors/getDialogStack";
import { getMenuItemType } from "model/selectors/getMenuItemType";
import React from "react";
import { map2obj } from "utils/objects";
import { interpretScreenXml } from "xmlInterpreters/screenXml";
import { getFormScreen } from "../../selectors/FormScreen/getFormScreen";
import { getScreenParameters } from "../../selectors/FormScreen/getScreenParameters";
import { getApi } from "../../selectors/getApi";
import { getMenuItemId } from "../../selectors/getMenuItemId";
import { getOpenedScreen } from "../../selectors/getOpenedScreen";
import { getSessionId } from "../../selectors/getSessionId";
import { IFormScreenLifecycle02 } from "../types/IFormScreenLifecycle";
import { getIsFormScreenDirty } from "model/selectors/FormScreen/getisFormScreenDirty";
import { errDialogPromise } from "../ErrorDialog";
import { handleError } from "model/actions/handleError";

enum IQuestionSaveDataAnswer {
  Cancel = 0,
  NoSave = 1,
  Save = 2
}

export class FormScreenLifecycle02 implements IFormScreenLifecycle02 {
  $type_IFormScreenLifecycle: 1 = 1;

  @computed get isWorking() {
    return this.inFlow > 0;
  }
  @observable inFlow = 0;

  *onFlushData(): Generator<unknown, any, unknown> {
    yield* this.flushData();
  }

  *onCreateRow(
    entity: string,
    gridId: string
  ): Generator<unknown, any, unknown> {
    yield* this.createRow(entity, gridId);
  }

  *onDeleteRow(
    entity: string,
    rowId: string
  ): Generator<unknown, any, unknown> {
    yield* this.deleteRow(entity, rowId);
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

  *onRequestScreenClose(): Generator<unknown, any, unknown> {
    if (!getIsFormScreenDirty(this)) {
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
    if (!getIsFormScreenDirty(this)) {
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

  *start(initUIResult: any): Generator {
    // yield* this.initUI();
    yield* this.applyInitUIResult({ initUIResult });
    if (!this.isReadData) {
      yield* this.loadData();
    }
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

  *destroyUI() {
    try {
      this.inFlow++;
      const api = getApi(this);
      yield api.destroyUI({ FormSessionId: getSessionId(this) });
    } finally {
      this.inFlow--;
    }
  }

  *applyInitUIResult(args: { initUIResult: any }) {
    const openedScreen = getOpenedScreen(this);
    const screenXmlObj = args.initUIResult.formDefinition;
    const screen = interpretScreenXml(
      screenXmlObj,
      this,
      args.initUIResult.panelConfigurations,
      args.initUIResult.sessionId
    );
    openedScreen.content.setFormScreen(screen);
    screen.printMasterDetailTree();
    yield* this.applyData(args.initUIResult.data);
    getDataViewList(this).forEach(dv => dv.start());
  }

  *loadData() {
    try {
      this.inFlow++;
      const api = getApi(this);
      const formScreen = getFormScreen(this);
      for (let rootDataView of formScreen.rootDataViews) {
        const loadedData = yield api.getRows({
          MenuId: getMenuItemId(rootDataView),
          DataStructureEntityId: getDataStructureEntityId(rootDataView),
          Filter: "",
          Ordering: [],
          RowLimit: 999999,
          ColumnNames: getColumnNamesToLoad(rootDataView),
          MasterRowId: undefined
        });
        rootDataView.dataTable.clear();
        rootDataView.dataTable.setRecords(loadedData);
        rootDataView.selectFirstRow();
      }
    } finally {
      this.inFlow--;
    }
  }

  *flushData() {
    try {
      this.inFlow++;
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
          yield* processCRUDResult(dataView, updateObjectResult);
        }
      }
    } finally {
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
      yield* processCRUDResult(this, deleteObjectResult);
    } finally {
      this.inFlow--;
    }
  }

  *saveSession() {
    try {
      this.inFlow++;
      const api = getApi(this);
      yield api.saveSessionQuery(getSessionId(this));
      const result = yield api.saveSession(getSessionId(this));
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
        const api = getApi(this);
        const result = yield api.refreshSession(getSessionId(this));
        yield* this.applyData(result);
        getFormScreen(this).setDirty(false);
      } else {
        yield* this.loadData();
      }
    } finally {
      this.inFlow--;
    }
  }

  *executeAction(
    gridId: string,
    entity: string,
    action: IAction,
    selectedItems: string[]
  ) {
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

      yield* processActionResult(action)(result);
    } finally {
      this.inFlow--;
    }
  }

  *closeForm() {
    try {
      this.inFlow++;
      yield* this.destroyUI();
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

  *applyData(data: any): Generator {
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

  get isReadData() {
    return !getOpenedScreen(this).dontRequestData;
  }

  parent?: any;
}
