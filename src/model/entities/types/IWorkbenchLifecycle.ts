import {IDialogInfo, IOpenedScreen} from "./IOpenedScreen";
import {IMainMenuItemType} from "./IMainMenu";
import {IUserInfo} from "model/entities/types/IUserInfo";
import {IPortalSettings} from "model/entities/types/IPortalSettings";

export interface IWorkbenchLifecycle {
  $type_IWorkbenchLifecycle: 1;

  onMainMenuItemClick(args: { event: any; item: any, idParameter: string | undefined }): Generator;
  onMainMenuItemIdClick(args: { event: any; itemId: any, idParameter: string }): Generator;
  onWorkQueueListItemClick(event: any, item: any): Generator;
  onChatroomsListItemClick(event: any, item: any): Generator;
  onScreenTabHandleClick(event: any, openedScreen: IOpenedScreen): Generator;
  userInfo: IUserInfo | undefined;
  logoUrl: string | undefined;
  customAssetsRoute: string | undefined;
  portalSettings: IPortalSettings | undefined;
  openNewForm(
    id: string,
    type: IMainMenuItemType,
    label: string,
    dontRequestData: boolean,
    dialogInfo: IDialogInfo | undefined,
    parameters: { [key: string]: any },
    parentContext: any,
    additionalRequestParameters?: object | undefined,
  ): Generator;

  openNewUrl(url: string, title: string): Generator;

  closeForm(openedScreen: IOpenedScreen): Generator;

  run(): Generator;
  parent?: any;
}

export const isIWorkbenchLifecycle = (o: any): o is IWorkbenchLifecycle =>
  o.$type_IWorkbenchLifecycle;
