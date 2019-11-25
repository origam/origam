import { getWorkbench } from "model/selectors/getWorkbench";
import { getIsCurrentScreenFull } from "model/selectors/Workbench/getIsCurrentScreenFull";

export function onFullscreenClick(ctx: any) {
  return function onFullscreenClick(event: any) {
    getWorkbench(ctx).setFullscreen(!getIsCurrentScreenFull(ctx));
  };
}
