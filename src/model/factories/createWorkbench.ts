import { Workbench } from "../Workbench";
import { WorkbenchLifecycle } from '../WorkbenchLifecycle';

export function createWorkbench() {
  return new Workbench({
    workbenchLifecycle: new WorkbenchLifecycle()
  });
}
