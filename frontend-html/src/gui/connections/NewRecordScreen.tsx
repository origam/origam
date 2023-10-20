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
import { getNewRecordScreenData } from "model/selectors/getNewRecordScreenData";
import { runGeneratorInFlowWithHandler } from "utils/runInFlowWithHandler";
import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";
import S from "gui/Workbench/ScreenArea/ScreenArea.module.scss";
import { T } from "utils/translation";
import React from "react";
import { getMainMenuItemById } from "model/selectors/MainMenu/getMainMenuItemById";
import { getWorkbenchLifecycle } from "model/selectors/getWorkbenchLifecycle";
import { DialogInfo } from "model/entities/OpenedScreen";
import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";
import { IProperty } from "model/entities/types/IProperty";

export class NewRecordScreen {
  private _width: number;
  private _height: number;
  private _menuItemId: string;

  constructor(args: {
    width: number,
    height: number,
    menuItemId: string}
  ) {
    this._width = args.width;
    this._height = args.height;
    this._menuItemId = args.menuItemId;
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
}

export function getNewRecordScreenButtons(openedScreen: IOpenedScreen) {

  function afterClose() {
    const newRecordScreenData = getNewRecordScreenData(openedScreen);
    if (newRecordScreenData.parentTablePanelView) {
      newRecordScreenData.parentTablePanelView.isEditing = true;
      newRecordScreenData.parentTablePanelView = undefined;
    }
  }

  async function onSaveClick() {
    await runGeneratorInFlowWithHandler({
      ctx: openedScreen,
      generator: function*() {
        const formScreenLifecycle = getFormScreenLifecycle(openedScreen.content.formScreen);
        yield*formScreenLifecycle.onSaveSession();
        yield*formScreenLifecycle.closeForm();
        afterClose();
      }()
    });
  }

  async function onCloseClick() {
    await runGeneratorInFlowWithHandler({
      ctx: openedScreen,
      generator: function*() {
        const formScreenLifecycle = getFormScreenLifecycle(openedScreen.content.formScreen);
        yield*formScreenLifecycle.closeForm();
        afterClose();
      }()
    });
  }

  return (
    <>
      <button
        className={S.workflowActionBtn}
        onClick={onCloseClick}
      >
        {T("Close", "button_close")}
      </button>
      <button
        className={S.workflowActionBtn}
        onClick={onSaveClick}
      >
        {T("Save", "save_tool_tip")}
      </button>
    </>
  );
}

export function makeOnAddNewRecordClick(property: IProperty){
  return async function onAddNewRecordClick(){
    if(!property.lookup?.newRecordScreen){
      throw new Error("newRecordScreen not found on property " + property.id);
    }
    const newRecordScreen = property.lookup.newRecordScreen;
    const menuItem = getMainMenuItemById(property, property.lookup.newRecordScreen.menuItemId);
    const workbenchLifecycle = getWorkbenchLifecycle(property);
    const dialogInfo = new DialogInfo(newRecordScreen.width, newRecordScreen.height);
    const newRecordScreenData = getNewRecordScreenData(property);
    const tablePanelView = getTablePanelView(property)!;
    newRecordScreenData.parentTablePanelView = tablePanelView;
    tablePanelView.isEditing = false;
    await runGeneratorInFlowWithHandler({
      ctx: property,
      generator: function*() {
        yield*workbenchLifecycle.openNewForm(
          property!.lookup!.newRecordScreen!.menuItemId,
          menuItem.attributes.type,
          menuItem.attributes.label,
          menuItem.attributes.lazyLoading === "true",
          dialogInfo,
          {},
          undefined,
          undefined,
          undefined,
          undefined,
          undefined,
          undefined,
          true,
          true
        );
      }()
    });
  }
}
