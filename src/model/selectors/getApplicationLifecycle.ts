import { IApplicationLifecycle } from "../types/IApplicationLifecycle";
import { getApplication } from "./getApplication";

export function getApplicationLifecycle(ctx: any): IApplicationLifecycle {
  return getApplication(ctx).applicationLifecycle;
}
