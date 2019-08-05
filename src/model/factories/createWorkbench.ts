import { Workbench } from "../entities/Workbench";
import { WorkbenchLifecycle } from '../entities/WorkbenchLifecycle';
import { ClientFulltextSearch } from '../entities/ClientFulltextSearch';

export function createWorkbench() {
  return new Workbench({
    workbenchLifecycle: new WorkbenchLifecycle(),
    clientFulltextSearch: new ClientFulltextSearch()
  });
}
