import {IActionResultType} from "../SelectionDialog/types";
import {getWorkbenchLifecycle} from "../../selectors/getWorkbenchLifecycle";
import {DialogInfo} from "model/entities/OpenedScreen";
import {closeForm} from "../closeForm";
import {getActionCaption} from "model/selectors/Actions/getActionCaption";
import {IMainMenuItemType} from "model/entities/types/IMainMenu";
import {IDialogInfo} from "model/entities/types/IOpenedScreen";

import actions from "model/actions-tree";
import {IUrlUpenMethod} from "model/entities/types/IUrlOpenMethod";
import {openNewUrl} from "../Workbench/openNewUrl";
import {ICRUDResult, processCRUDResult} from "../DataLoading/processCRUDResult";
import {IRefreshOnReturnType} from "model/entities/WorkbenchLifecycle/WorkbenchLifecycle";
import {IDataView} from "../../entities/types/IDataView";
import {getDataViewByModelInstanceId} from "../../selectors/DataView/getDataViewByModelInstanceId";

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
    registerSession?: true,
    refreshOnReturnType?: IRefreshOnReturnType
  ): Generator; //boolean
}

export interface IOpenNewUrl {
  (url: string, urlOpenMethod: IUrlUpenMethod, title: string): Generator;
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
  (result: ICRUDResult): Generator;
}

export function new_ProcessActionResult(ctx: any) {
  const workbenchLifecycle = getWorkbenchLifecycle(ctx);
  const getPanelFunc = (modelInstanceId: string) => getDataViewByModelInstanceId(ctx, modelInstanceId)!;
  return processActionResult2({
    getPanelFunc: getPanelFunc,
    openNewForm: workbenchLifecycle.openNewForm,
    openNewUrl: openNewUrl(ctx),
    closeForm: closeForm(ctx),
    refreshForm: actions.formScreen.refresh(ctx),
    getActionCaption: () => getActionCaption(ctx),
    processCRUDResult: (crudResult: ICRUDResult) => processCRUDResult(ctx, crudResult)
  });
}

export function processActionResult2(dep: {
  getPanelFunc: (modelInstanceId: string) => IDataView;
  openNewForm: IOpenNewForm;
  openNewUrl: IOpenNewUrl;
  closeForm: ICloseForm;
  refreshForm: IRefreshForm;
  getActionCaption: IGetActionCaption;
  processCRUDResult: IProcessCRUDResult;
}) {
  return function* processActionResult2(actionResultList: any[]) {
    for (let actionResultItem of actionResultList) {
      switch (actionResultItem.type) {
        case IActionResultType.OpenForm: {
          const { request, refreshOnReturnType } = actionResultItem;
          const {
            objectId,
            typeString,
            dataRequested,
            parameters,
            isModalDialog,
            dialogWidth,
            dialogHeight,
            caption
          } = request;
          const dialogInfo = isModalDialog ? new DialogInfo(dialogWidth, dialogHeight) : undefined;
          yield* dep.openNewForm(
            objectId,
            typeString,
            caption || dep.getActionCaption(),
            !dataRequested,
            dialogInfo,
            parameters,
            undefined,
            undefined,
            undefined,
            refreshOnReturnType
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
        case IActionResultType.UpdateData: {
          yield* dep.processCRUDResult(actionResultItem.changes);
          break;
        }
        case IActionResultType.OpenUrl: {
          yield* dep.openNewUrl(
            actionResultItem.url,
            actionResultItem.urlOpenMethod,
            dep.getActionCaption()
          );
          break;
        }
        case IActionResultType.Script: {
          try {
            // eslint-disable-next-line no-new-func
            const actionScript = new Function("getPanel", actionResultItem.script);
            actionScript(dep.getPanelFunc);
          }catch (e) {
            let message = "An error occurred while executing custom script: "+actionResultItem.script+", \n"+e.message;
            if(e.stackTrace)
              message +=(", \n"+e.stackTrace);
            throw new Error(message)
          }
          break;
        }
      }
    }
  };
}
