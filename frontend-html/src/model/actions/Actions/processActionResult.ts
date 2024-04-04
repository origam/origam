/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

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

import { IActionResultType } from "../SelectionDialog/types";
import { getWorkbenchLifecycle } from "../../selectors/getWorkbenchLifecycle";
import { DialogInfo } from "model/entities/OpenedScreen";
import { closeForm } from "../closeForm";
import { getActionCaption } from "model/selectors/Actions/getActionCaption";
import { IMainMenuItemType } from "model/entities/types/IMainMenu";
import { IDialogInfo } from "model/entities/types/IOpenedScreen";

import actions from "model/actions-tree";
import { IUrlOpenMethod } from "model/entities/types/IUrlOpenMethod";
import { openNewUrl } from "../Workbench/openNewUrl";
import { ICRUDResult, processCRUDResult } from "../DataLoading/processCRUDResult";
import { IRefreshOnReturnType } from "model/entities/WorkbenchLifecycle/WorkbenchLifecycle";
import { IDataView } from "../../entities/types/IDataView";
import { getDataViewByModelInstanceId } from "../../selectors/DataView/getDataViewByModelInstanceId";
import { getOpenedScreen } from "../../selectors/getOpenedScreen";
import { getMainMenuItemById } from "model/selectors/MainMenu/getMainMenuItemById";

export interface IOpenNewForm {
  (
    args: {
      id: string,
      type: IMainMenuItemType,
      label: string,
      isLazyLoading: boolean,
      dialogInfo: IDialogInfo | undefined,
      parameters: { [key: string]: any },
      parentContext?: any,
      requestParameters?: object | undefined,
      formSessionId?: string,
      isSessionRebirth?: boolean,
      isSleepingDirty?: boolean,
      refreshOnReturnType?: IRefreshOnReturnType,
      onClose?: ()=> void
    }
  ): Generator; //boolean
}

export interface IOpenNewUrl {
  (url: string, urlOpenMethod: IUrlOpenMethod, title: string): Generator;
}

export interface IGetActionCaption {
  (): string;
}

export interface ICloseForm {
  (): Generator;
}

export interface IRefreshForm {
  (): Generator;
}

export interface IProcessCRUDResult {
  (data: { crudResults: ICRUDResult[], resortTables?: boolean }): Generator;
}

export interface IActionResult {
  changes: any[] | null;
  refreshOnReturnSessionId: string | null;
  refreshOnReturnType: IRefreshOnReturnType | undefined;
  request: any;
  script: string | null;
  type: IActionResultType;
  uiResult: any;
  url: string | null;
  urlOpenMethod: IUrlOpenMethod;
}

export function processActionResult(ctx: any) {
  const workbenchLifecycle = getWorkbenchLifecycle(ctx);
  const getPanelFunc = (modelInstanceId: string) => getDataViewByModelInstanceId(ctx, modelInstanceId)!;
  return internalProcessActionResult({
    getPanelFunc: getPanelFunc,
    openNewForm: workbenchLifecycle.openNewForm,
    openNewUrl: openNewUrl(ctx),
    closeForm: closeForm(ctx),
    refreshForm: actions.formScreen.refresh(ctx),
    getActionCaption: () => getActionCaption(ctx),
    processCRUDResult: (data: { crudResults: ICRUDResult[], resortTables?: boolean }) => processCRUDResult(ctx, data.crudResults, data.resortTables),
    parentContext: ctx
  });
}

function internalProcessActionResult(dep: {
  getPanelFunc: (modelInstanceId: string) => IDataView;
  openNewForm: IOpenNewForm;
  openNewUrl: IOpenNewUrl;
  closeForm: ICloseForm;
  refreshForm: IRefreshForm;
  getActionCaption: IGetActionCaption;
  processCRUDResult: IProcessCRUDResult;
  parentContext: any
}) {
  return function*internalProcessActionResult(actionResultList: IActionResult[]) {
    const indexedList = actionResultList.map((item, index) => [index, item]);
    indexedList.sort((a: any, b: any) => {
      if (a[1].type === IActionResultType.DestroyForm) return -1;
      if (b[1].type === IActionResultType.DestroyForm) return 1;
      return a[0] - b[0]
    })
    let onCloseUserScript;
    const willOpenNewWindow = actionResultList.some(x => x.type === IActionResultType.OpenForm);
    if (willOpenNewWindow) {
      onCloseUserScript = actionResultList.find(x => x.type === IActionResultType.Script);
    }
    for (let actionResultItem of indexedList.map(item => item[1] as IActionResult)) {
      switch (actionResultItem.type) {
        case IActionResultType.OpenForm: {
          const menuItem = getMainMenuItemById(dep.parentContext, actionResultItem.request.objectId);
          const lazyLoading = menuItem
            ? menuItem?.attributes?.lazyLoading === "true"
            : false;
          const {request, refreshOnReturnType} = actionResultItem;
          const {
            objectId,
            typeString,
            parameters,
            isModalDialog,
            dialogWidth,
            dialogHeight,
            caption
          } = request;
          const dialogInfo = isModalDialog ? new DialogInfo(dialogWidth, dialogHeight) : undefined;
          yield*dep.openNewForm(
            {
              id: objectId,
              type: typeString,
              label: caption || dep.getActionCaption(),
              isLazyLoading: lazyLoading,
              dialogInfo: dialogInfo,
              parameters: parameters,
              parentContext: dep.parentContext,
              requestParameters: actionResultItem.request,
              refreshOnReturnType: refreshOnReturnType,
              onClose: onCloseUserScript
                ? () => processScript(onCloseUserScript, dep.getPanelFunc)
                : undefined
            }
          );
          break;
        }
        case IActionResultType.DestroyForm: {
          yield*dep.closeForm();
          break;
        }
        case IActionResultType.RefreshData: {
          yield*dep.refreshForm();
          break;
        }
        case IActionResultType.UpdateData: {
          yield*dep.processCRUDResult(
            {crudResults: actionResultItem.changes!, resortTables: true}
          );
          break;
        }
        case IActionResultType.OpenUrl: {
          yield*dep.openNewUrl(
            actionResultItem.url!,
            actionResultItem.urlOpenMethod,
            actionResultItem.request.caption
          );
          if (getOpenedScreen(dep.parentContext).isDialog) {
            yield*dep.closeForm();
          }
          break;
        }
        case IActionResultType.Script: {
          processScript(actionResultItem, dep.getPanelFunc);
          break;
        }
      }
    }
  };
}

function processScript(actionResultItem: any, getPanelFunc: (modelInstanceId: string) => IDataView) {
  try {
    // eslint-disable-next-line no-new-func
    const actionScript = new Function("getPanel", actionResultItem.script);
    actionScript(getPanelFunc);
  } catch (e: any) {
    let message = "An error occurred while executing custom script: " + actionResultItem.script + ", \n" + e.message;
    if (e.stackTrace)
      message += (", \n" + e.stackTrace);
    throw new Error(message)
  }
}
