import { getWorkbench } from "../getWorkbench";

export function getChatrooms(ctx: any) {
  return getWorkbench(ctx).chatrooms;
}