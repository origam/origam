/*
Copyright 2005 - 2023 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

import { IOpenedScreen } from "model/entities/types/IOpenedScreen";
import { runGeneratorInFlowWithHandler } from "utils/runInFlowWithHandler";
import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";
import SA from "gui/Workbench/ScreenArea/ScreenArea.module.scss";
import S from "gui/connections/NewRecordScreen.module.scss";
import { T } from "utils/translation";
import React from "react";
import { getMainMenuItemById } from "model/selectors/MainMenu/getMainMenuItemById";
import { getWorkbenchLifecycle } from "model/selectors/getWorkbenchLifecycle";
import { DialogInfo } from "model/entities/OpenedScreen";
import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";
import { IProperty } from "model/entities/types/IProperty";
import { onFieldChange } from "model/actions-ui/DataView/TableView/onFieldChange";
import { getWorkbench } from "model/selectors/getWorkbench";
import { NewRecordScreenData } from "model/entities/NewRecordScreenData";
import { getDataView } from "model/selectors/DataView/getDataView";
import cx from "classnames";
import { getFormFocusManager } from "model/selectors/DataView/getFormFocusManager";
import { getApi } from "model/selectors/getApi";
import { getDataStructureEntityId } from "model/selectors/DataView/getDataStructureEntityId";
import { getEntity } from "model/selectors/DataView/getEntity";
import { getSessionId } from "model/selectors/getSessionId";
import { getMenuItemId } from "model/selectors/getMenuItemId";

export class NewRecordScreen {
  private _width: number;
  private _height: number;
  private _menuItemId: string;
  private _parameterMappings:  { [p: string]: any };

  constructor(args: {
    width: number;
    menuItemId: any;
    height: number,
    parameterMappings: { [p: string]: any }
  }
  ) {
    this._width = args.width;
    this._height = args.height;
    this._menuItemId = args.menuItemId;
    this._parameterMappings = args.parameterMappings;
  }

  get width() {
    return this._width;
  }

  get height() {
    return this._height;
  }

  get menuItemId() {
    return this._menuItemId;
  }

  get parameterMappings() {
    return this._parameterMappings;
  }
}

export function getNewRecordScreenButtons(openedScreen: IOpenedScreen) {
  return (
    <>
      <button
        className={SA.workflowActionBtn}
        onClick={() => onCloseClick(openedScreen)}
      >
        {T("Close", "button_close")}
      </button>
      <button
        id={S.defaultSaveButton}
        className={cx(SA.workflowActionBtn)}
        onClick={() => onSaveClick(openedScreen)}
      >
        {T("Save", "save_tool_tip")}
      </button>
    </>
  );
}

export async function onSaveClick(openedScreen: IOpenedScreen) {
  await runGeneratorInFlowWithHandler({
    ctx: openedScreen,
    generator: function*() {
      const rootDataView = openedScreen.content.formScreen?.rootDataViews[0];
      if (!rootDataView) {
        throw new Error("rootDataView not found")
      }
      if (rootDataView.dataTable.rows.length !== 1) {
        throw new Error("first row not found")
      }
      const firstRow = rootDataView.dataTable.rows[0];
      const insertedRowId = rootDataView.dataTable.getRowId(firstRow);
      const formScreenLifecycle = getFormScreenLifecycle(openedScreen.content.formScreen);
      yield*formScreenLifecycle.onSaveSession();
      yield*formScreenLifecycle.closeForm();
      yield*updateComboBoxValue(insertedRowId, openedScreen);
      afterClose(openedScreen);
    }()
  });
}

function afterClose(openedScreen: IOpenedScreen) {
  const workbench = getWorkbench(openedScreen);
  const newRecordScreenData = workbench.newRecordScreenData;
  if (!newRecordScreenData) {
    throw new Error("newRecordScreenData was not found");
  }
  const dataView = getDataView(newRecordScreenData.comboBoxProperty);
  if (dataView.isTableViewActive()) {
    const comboBoxTablePanelView = getTablePanelView(newRecordScreenData.comboBoxProperty);
    comboBoxTablePanelView.isEditing = true;
  }
  else if (dataView.isFormViewActive()) {
    const formFocusManager = getFormFocusManager(dataView);
    formFocusManager.refocusLast();
  }
  workbench.newRecordScreenData = undefined;
}

async function onCloseClick(openedScreen: IOpenedScreen) {
  await runGeneratorInFlowWithHandler({
    ctx: openedScreen,
    generator: function*() {
      const formScreenLifecycle = getFormScreenLifecycle(openedScreen.content.formScreen);
      yield*formScreenLifecycle.closeForm();
      afterClose(openedScreen);
    }()
  });
}

function*updateComboBoxValue(insertedRowId: string, openedScreen: IOpenedScreen) {
  const workbench = getWorkbench(openedScreen);
  const newRecordScreenData = workbench.newRecordScreenData;
  if (!newRecordScreenData) {
    throw new Error("newRecordScreenData was not found");
  }
  yield onFieldChange(newRecordScreenData.comboBoxProperty)({
    event: undefined,
    row: newRecordScreenData.comboBoxRow,
    property: newRecordScreenData.comboBoxProperty,
    value: insertedRowId,
  });
}


export function makeOnAddNewRecordClick(property: IProperty){
  return async function onAddNewRecordClick(searchText?: string){
    if (!property.lookup?.newRecordScreen) {
      throw new Error("newRecordScreen not found on property " + property.id);
    }
    const newRecordScreen = property.lookup.newRecordScreen;
    const menuItem = getMainMenuItemById(property, property.lookup.newRecordScreen.menuItemId);
    const workbenchLifecycle = getWorkbenchLifecycle(property);
    const dialogInfo = new DialogInfo(newRecordScreen.width, newRecordScreen.height);
    const selectedRow = getDataView(property).selectedRow!;
    getWorkbench(property).newRecordScreenData = new NewRecordScreenData(property, selectedRow);
    const dataView = getDataView(property);
    const tablePanelView = getTablePanelView(property)!;
    tablePanelView.isEditing = false;
    await runGeneratorInFlowWithHandler({
      ctx: property,
      generator: function*() {
        const api = getApi(property);
        const parameters = property.lookup?.dropDownParameters
            .reduce((paramsObj, param) => {
              paramsObj[param.parameterName] = param.fieldName;
              return paramsObj;
            }, {} as {[p:string]: string});
        const newRecordInitialValues = (yield api.getLookupNewRecordInitialValues({
          Property: property.id,
          Id: dataView.selectedRowId!,
          LookupId: property.lookupId!,
          Parameters: parameters,
          ParameterMappings: property.lookup?.newRecordScreen?.parameterMappings,
          SearchText: searchText,
          DataStructureEntityId: getDataStructureEntityId(property),
          Entity:  getEntity(property),
          SessionFormIdentifier: getSessionId(property),
          MenuId: getMenuItemId(property)
        })) as { [key: string]: string };

        yield*workbenchLifecycle.openNewForm(
          {
            id: property!.lookup!.newRecordScreen!.menuItemId,
            type: menuItem.attributes.type,
            label: menuItem.attributes.label,
            isLazyLoading: menuItem.attributes.lazyLoading === "true",
            dialogInfo: dialogInfo,
            parameters: { },
            isSingleRecordEdit: true,
            newRecordInitialValues: newRecordInitialValues
          }
        );
      }()
    });
  }
}
