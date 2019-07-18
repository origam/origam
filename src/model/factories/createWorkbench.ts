import { Workbench } from "../entities/Workbench";
import { WorkbenchLifecycle } from '../entities/WorkbenchLifecycle';

export function createWorkbench() {
  return new Workbench({
    workbenchLifecycle: new WorkbenchLifecycle()
  });
}
