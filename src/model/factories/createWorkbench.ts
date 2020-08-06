import { Workbench } from "../entities/Workbench";

import { ClientFulltextSearch } from "../entities/ClientFulltextSearch";
import { WorkbenchLifecycle } from "model/entities/WorkbenchLifecycle/WorkbenchLifecycle";
import { MainMenuEnvelope } from "../entities/MainMenu";
import { OpenedScreens } from "model/entities/OpenedScreens";
import { WorkQueues } from "model/entities/WorkQueues";
import { RecordInfo } from "model/entities/RecordInfo";
import { LookupListCacheMulti } from "../../modules/Lookup/LookupListCacheMulti";
import { Clock } from "../../modules/Lookup/Clock";
import $root from "../../rootContainer";
import { SCOPE_Workbench } from "../../modules/Workbench/WorkbenchModule";
import {registerScope} from "../../dic/Container";

export function createWorkbench() {
  const clock = new Clock();
  const workbenchLookupListCache = new LookupListCacheMulti(clock);

  const instance = new Workbench({
    mainMenuEnvelope: new MainMenuEnvelope(),
    workbenchLifecycle: new WorkbenchLifecycle(),
    clientFulltextSearch: new ClientFulltextSearch(),
    openedScreens: new OpenedScreens(),
    openedDialogScreens: new OpenedScreens(),
    workQueues: new WorkQueues(),
    recordInfo: new RecordInfo(),

    lookupListCache: workbenchLookupListCache,
  });
  workbenchLookupListCache.startup();
  const $workbench = $root.beginLifetimeScope(SCOPE_Workbench);
  registerScope(instance, $workbench);
  return instance;
}
