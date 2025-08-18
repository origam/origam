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

import { IMainMenuEnvelope } from "./IMainMenu";
import { IWorkbenchLifecycle } from "./IWorkbenchLifecycle";
import { ISearcher as ISearcher } from "./ISearcher";
import { IOpenedScreens } from "./IOpenedScreens";
import { IWorkQueues } from "./IWorkQueues";
import { IRecordInfo } from "./IRecordInfo";
import { LookupListCacheMulti } from "../../../modules/Lookup/LookupListCacheMulti";
import { IMultiLookupEngine } from "modules/Lookup/LookupModule";
import { Chatrooms } from "../Chatrooms";
import { Notifications } from "../Notifications";
import { Favorites } from "model/entities/Favorites";
import { SidebarState } from "../SidebarState";
import { About } from "model/entities/AboutInfo";
import { NewRecordScreenData } from "model/entities/NewRecordScreenData";

export interface IWorkbenchData {
  mainMenuEnvelope: IMainMenuEnvelope;
  workbenchLifecycle: IWorkbenchLifecycle;
  searcher: ISearcher;
  openedScreens: IOpenedScreens;
  openedDialogScreens: IOpenedScreens;
  workQueues: IWorkQueues;
  chatrooms: Chatrooms;
  notifications: Notifications;
  recordInfo: IRecordInfo;
  favorites: Favorites;
  sidebarState: SidebarState;
  about: About;
  lookupListCache: LookupListCacheMulti;
  lookupMultiEngine: IMultiLookupEngine;
  newRecordScreenData?: NewRecordScreenData
}

export interface IWorkbench extends IWorkbenchData {
  $type_IWorkbench: 1;
  isFullScreen: boolean;

  mainMenuEnvelope: IMainMenuEnvelope;

  run(): Generator;

  setFullscreen(state: boolean): void;

  openedScreenIdSet: Set<string>;

  parent?: any;
}

export const isIWorkbench = (o: any): o is IWorkbench => o.$type_IWorkbench;
