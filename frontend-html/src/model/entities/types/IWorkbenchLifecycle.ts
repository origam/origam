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

import { IDialogInfo, IOpenedScreen } from "./IOpenedScreen";
import { IMainMenuItemType } from "./IMainMenu";
import { IUserInfo } from "model/entities/types/IUserInfo";
import { IPortalSettings } from "model/entities/types/IPortalSettings";
import { EventHandler } from "utils/EventHandler";
import { IRefreshOnReturnType } from "model/entities/WorkbenchLifecycle/WorkbenchLifecycle";
import { KeyBuffer } from "model/entities/WorkbenchLifecycle/KeyBuffer";

export interface IWorkbenchLifecycle {
  $type_IWorkbenchLifecycle: 1;

  onMainMenuItemClick(args: {
    event: any;
    item: any;
    idParameter: string | undefined;
    isSingleRecordEdit?: boolean;
  }): Generator;

  onMainMenuItemIdClick(args: {
    event: any;
    itemId: any;
    idParameter: string | undefined;
    isSingleRecordEdit?: boolean;
  }): Generator;

  onWorkQueueListItemClick(event: any, item: any): Generator;

  onChatroomsListItemClick(event: any, item: any): Generator;

  onScreenTabHandleClick(event: any, openedScreen: IOpenedScreen): Generator;

  userInfo: IUserInfo | undefined;
  logoUrl: string | undefined;
  customAssetsRoute: string | undefined;
  portalSettings: IPortalSettings | undefined;
  keyBuffer: KeyBuffer;
  mainMenuItemClickHandler: EventHandler;

  openNewForm(args: {
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
      isSingleRecordEdit?: boolean,
      newRecordInitialValues?: {[p:string]: string},
      onClose?: ()=> void
    }
  ): Generator;

  openNewUrl(url: string, title: string): Generator;

  closeForm(openedScreen: IOpenedScreen): Generator;

  run(): Generator;

  parent?: any;
}

export const isIWorkbenchLifecycle = (o: any): o is IWorkbenchLifecycle =>
  o.$type_IWorkbenchLifecycle;
