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

import { IFormScreenEnvelope } from "./IFormScreen";
import { IMainMenuItemType } from "./IMainMenu";
import { EventHandler } from "utils/EventHandler";

export interface IDialogInfo {
  width: number;
  height: number;
}


export interface IOpenedScreenData {
  menuItemId: string;
  menuItemType: IMainMenuItemType;
  lazyLoading: boolean;
  dialogInfo?: IDialogInfo;
  order: number;
  tabTitle: string;
  content: IFormScreenEnvelope;
  parameters: { [key: string]: any };
  isSleeping?: boolean;
  isSleepingDirty?: boolean;
  isNewRecordScreen?: boolean;
}

export interface IOpenedScreen extends IOpenedScreenData {
  parentContext: IOpenedScreen | undefined;
  $type_IOpenedScreen: 1;

  isActive: boolean;
  isDialog: boolean;
  isClosed: boolean;
  stackPosition: number;
  isBeingClosed: boolean;
  formTitle: string;
  canRefresh: boolean;

  setActive(state: boolean): void;
  activationHandler: EventHandler;

  setContent(screen: IFormScreenEnvelope): void;

  screenUrl?: string;
  parent?: any;
  hasDynamicTitle: boolean;

  onWindowMove(top: number, left: number): void;
  get positionOffset(): {[key: string]: number};
}

export const isIOpenedScreen = (o: any): o is IOpenedScreen =>
  o?.$type_IOpenedScreen;
