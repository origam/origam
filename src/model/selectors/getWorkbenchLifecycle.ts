import { getWorkbench } from "./getWorkbench";

export function getWorkbenchLifecycle(ctx: any) {
  return getWorkbench(ctx).workbenchLifecycle;
}
