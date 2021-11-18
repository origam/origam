import { getFormScreen } from "./getFormScreen";

export function getIsSuppressRefresh(ctx: any) {
  return getFormScreen(ctx).suppressRefresh;
}
