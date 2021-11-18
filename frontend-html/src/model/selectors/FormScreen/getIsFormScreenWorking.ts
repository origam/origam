import {getFormScreenLifecycle} from "./getFormScreenLifecycle";

export function getIsFormScreenWorkingDelayed(ctx: any) {
  return getFormScreenLifecycle(ctx).isWorkingDelayed;
}