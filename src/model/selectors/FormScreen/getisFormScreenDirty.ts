import { getFormScreen } from "./getFormScreen";

export function getIsFormScreenDirty(ctx: any) {
  return getFormScreen(ctx).isDirty;
}