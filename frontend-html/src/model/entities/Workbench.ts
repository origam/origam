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

import { IWorkbench, IWorkbenchData } from "./types/IWorkbench";
import { IMainMenuEnvelope } from "./types/IMainMenu";
import { IWorkbenchLifecycle } from "./types/IWorkbenchLifecycle";
import { action, computed, observable } from "mobx";
import { ISearcher } from "./types/ISearcher";
import { IOpenedScreens } from "./types/IOpenedScreens";
import { IWorkQueues } from "./types/IWorkQueues";
import { IRecordInfo } from "./types/IRecordInfo";
import { LookupListCacheMulti } from "../../modules/Lookup/LookupListCacheMulti";
import { IMultiLookupEngine } from "modules/Lookup/LookupModule";
import { Chatrooms } from "./Chatrooms";
import { Notifications } from "./Notifications";
import { Favorites } from "model/entities/Favorites";
import { SidebarState } from "./SidebarState";
import { About } from "model/entities/AboutInfo";
import { NewRecordScreenData } from "model/entities/NewRecordScreenData";

export class Workbench implements IWorkbench {
  $type_IWorkbench: 1 = 1;

  constructor(data: IWorkbenchData) {
    Object.assign(this, data);
    this.workbenchLifecycle.parent = this;
    this.searcher.parent = this;
    this.openedScreens.parent = this;
    this.openedDialogScreens.parent = this;
    this.workQueues.parent = this;
    this.chatrooms.parent = this;
    this.notifications.parent = this;
    this.recordInfo.parent = this;
    this.favorites.parent = this;
    this.sidebarState.parent = this;
    this.about.parent = this;
  }

  workbenchLifecycle: IWorkbenchLifecycle = null as any;
  searcher: ISearcher = null as any;
  mainMenuEnvelope: IMainMenuEnvelope = null as any;
  openedScreens: IOpenedScreens = null as any;
  openedDialogScreens: IOpenedScreens = null as any;
  workQueues: IWorkQueues = null as any;
  chatrooms!: Chatrooms;
  notifications: Notifications = null as any;
  recordInfo: IRecordInfo = null as any;
  lookupListCache: LookupListCacheMulti = null as any;
  lookupMultiEngine: IMultiLookupEngine = null as any;
  favorites: Favorites = null as any;
  sidebarState: SidebarState = null as any;
  about: About = null as any;
  newRecordScreenData?: NewRecordScreenData;

  @observable isFullScreen: boolean = false;

  @action.bound setFullscreen(state: boolean) {
    this.isFullScreen = state;
  }

  @computed get openedScreenIdSet() {
    return new Set(this.openedScreens.items.map((item) => item.menuItemId));
  }

  *run(): Generator {
    yield*this.workbenchLifecycle.run();
  }

  parent?: any;
}
