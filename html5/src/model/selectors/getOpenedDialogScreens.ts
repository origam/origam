import { getWorkbench } from "./getWorkbench";

export function getOpenedDialogScreens(ctx: any) {
  return getWorkbench(ctx).openedDialogScreens;
}