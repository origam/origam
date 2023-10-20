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

import { Workbench } from "../entities/Workbench";

import { Searcher } from "../entities/Searcher";
import { WorkbenchLifecycle } from "model/entities/WorkbenchLifecycle/WorkbenchLifecycle";
import { MainMenuEnvelope } from "../entities/MainMenu";
import { OpenedScreens } from "model/entities/OpenedScreens";
import { WorkQueues } from "model/entities/WorkQueues";
import { RecordInfo } from "model/entities/RecordInfo";
import { LookupListCacheMulti } from "../../modules/Lookup/LookupListCacheMulti";
import { Clock } from "../../modules/Lookup/Clock";
import $root from "../../rootContainer";
import { SCOPE_Workbench } from "../../modules/Workbench/WorkbenchModule";
import { registerScope } from "../../dic/Container";
import { createMultiLookupEngine } from "modules/Lookup/LookupModule";
import { getApi } from "model/selectors/getApi";
import { Chatrooms } from "model/entities/Chatrooms";
import { Notifications } from "model/entities/Notifications";
import { Favorites } from "model/entities/Favorites";
import { SidebarState } from "model/entities/SidebarState";
import { About } from "model/entities/AboutInfo";
import { NewRecordScreenData } from "model/entities/NewRecordScreenData";

export function createWorkbench() {
  const clock = new Clock();
  const workbenchLookupListCache = new LookupListCacheMulti(clock);
  const lookupMultiEngine = createMultiLookupEngine(() => getApi(instance));

  const instance = new Workbench({
    mainMenuEnvelope: new MainMenuEnvelope(),
    favorites: new Favorites(),
    workbenchLifecycle: new WorkbenchLifecycle(),
    searcher: new Searcher(),
    openedScreens: new OpenedScreens(),
    openedDialogScreens: new OpenedScreens(),
    workQueues: new WorkQueues(),
    chatrooms: new Chatrooms(),
    recordInfo: new RecordInfo(),
    notifications: new Notifications(),
    sidebarState: new SidebarState(),
    lookupListCache: workbenchLookupListCache,
    lookupMultiEngine,
    about: new About(),
    newRecordScreenData: new NewRecordScreenData()
  });
  workbenchLookupListCache.startup();
  lookupMultiEngine.startup();
  const $workbench = $root.beginLifetimeScope(SCOPE_Workbench);
  registerScope(instance, $workbench);
  return instance;
}
