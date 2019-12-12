import { Workbench } from "../entities/Workbench";

import { ClientFulltextSearch } from "../entities/ClientFulltextSearch";
import { WorkbenchLifecycle } from "model/entities/WorkbenchLifecycle/WorkbenchLifecycle";
import { MainMenuEnvelope } from "../entities/MainMenu";
import { OpenedScreens } from "model/entities/OpenedScreens";
import { WorkQueues } from "model/entities/WorkQueues";


export function createWorkbench() {
  return new Workbench({
    mainMenuEnvelope: new MainMenuEnvelope(),
    workbenchLifecycle: new WorkbenchLifecycle(),
    clientFulltextSearch: new ClientFulltextSearch(),
    openedScreens: new OpenedScreens(),
    openedDialogScreens: new OpenedScreens(),
    workQueues: new WorkQueues()
  });
}
