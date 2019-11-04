import { getFormScreenLifecycle } from "./getFormScreenLifecycle";

export function getIsFormScreenWorking(ctx: any) {
  return getFormScreenLifecycle(ctx).isWorking;
}