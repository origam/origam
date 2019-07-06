import { CWorkbench, IWorkbench } from "../types/IWorkbench";
import { getApplication } from "./getApplication";

export function getWorkbench(ctx: any): IWorkbench {
  /*let cn = ctx;
  while(cn) {
    if(cn.$type === CWorkbench) {
      return cn
    }
    cn = cn.parent;
  }*/
  const workbench = getApplication(ctx).workbench;
  if (!workbench) {
    throw new Error("No workbench in Application.");
  }
  return workbench;
}
