import { getWorkbench } from "../getWorkbench";


export function getMainMenuState(ctx: any) {
  return getWorkbench(ctx).sidebarState.mainMenuState;
}
