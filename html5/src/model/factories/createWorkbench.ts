import { Workbench } from "../entities/Workbench";

import { ClientFulltextSearch } from '../entities/ClientFulltextSearch';
import { WorkbenchLifecycle } from "model/entities/WorkbenchLifecycle/WorkbenchLifecycle";
import { MainMenuEnvelope } from '../entities/MainMenu';

export function createWorkbench() {
  return new Workbench({
    mainMenuEnvelope: new MainMenuEnvelope(),
    workbenchLifecycle: new WorkbenchLifecycle(),
    clientFulltextSearch: new ClientFulltextSearch()
  });
}
