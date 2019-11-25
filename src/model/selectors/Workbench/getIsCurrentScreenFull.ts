import { getWorkbench } from "../getWorkbench";

export function getIsCurrentScreenFull(ctx: any) {
  return getWorkbench(ctx).isFullScreen;
}