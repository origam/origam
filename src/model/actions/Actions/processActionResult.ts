import { IActionResultType } from "../SelectionDialog/types";
import { getWorkbenchLifecycle } from "../../selectors/getWorkbenchLifecycle";
import { DialogInfo } from "model/entities/OpenedScreen";
import { closeForm } from "../closeForm";
import { getActionCaption } from "model/selectors/Actions/getActionCaption";
import { IMainMenuItemType } from "model/entities/types/IMainMenu";
import { IDialogInfo } from "model/entities/types/IOpenedScreen";

import actions from 'model/actions-tree'

export interface IOpenNewForm {
  (
    id: string,
    type: IMainMenuItemType,
    label: string,
    dontRequestData: boolean,
    dialogInfo: IDialogInfo | undefined,
    parameters: { [key: string]: any },
    formSessionId?: string,
    isSessionRebirth?: boolean,
    registerSession?: true
  ): Generator; //boolean
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

export function new_ProcessActionResult($: any) {
  const workbenchLifecycle = getWorkbenchLifecycle($);
  return processActionResult2({
    openNewForm: workbenchLifecycle.openNewForm,
    closeForm: closeForm($),
    refreshForm: actions.formScreen.refresh($),
    getActionCaption: () => getActionCaption($)
  });
}

export function processActionResult2(dep: {
  openNewForm: IOpenNewForm;
  closeForm: ICloseForm;
  refreshForm: IRefreshForm;
  getActionCaption: IGetActionCaption;
}) {
  return function* processActionResult2(actionResultList: any[]) {
    for (let actionResultItem of actionResultList) {
      switch (actionResultItem.type) {
        case IActionResultType.OpenForm: {
          const { request } = actionResultItem;
          const {
            objectId,
            typeString,
            dataRequested,
            parameters,
            isModalDialog,
            dialogWidth,
            dialogHeight
          } = request;
          const dialogInfo = isModalDialog
            ? new DialogInfo(dialogWidth, dialogHeight)
            : undefined;
          yield* dep.openNewForm(
            objectId,
            typeString,
            dep.getActionCaption(),
            !dataRequested,
            dialogInfo,
            parameters
          );
          break;
        }
        case IActionResultType.DestroyForm: {
          yield* dep.closeForm();
          break;
        }
        case IActionResultType.RefreshData: {
          yield* dep.refreshForm();
          break;
        }
      }
    }
  };
}
