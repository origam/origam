import { getWorkbench } from "model/selectors/getWorkbench";

export function getNotifications(ctx: any) {
  return getWorkbench(ctx).notifications;
}