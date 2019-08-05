import { IWorkbench } from "../entities/types/IWorkbench";
import { getApplication } from "./getApplication";

export function getWorkbench(ctx: any): IWorkbench {
  const workbench = getApplication(ctx).workbench;
  if (!workbench) {
    throw new Error("No workbench in Application.");
  }
  return workbench;
}
