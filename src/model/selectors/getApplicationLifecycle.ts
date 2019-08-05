import { IApplicationLifecycle } from "../entities/types/IApplicationLifecycle";
import { getApplication } from "./getApplication";

export function getApplicationLifecycle(ctx: any): IApplicationLifecycle {
  return getApplication(ctx).applicationLifecycle;
}
