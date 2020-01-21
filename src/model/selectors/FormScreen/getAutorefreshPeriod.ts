import { getFormScreen } from "./getFormScreen";

export function getAutorefreshPeriod(ctx: any) {
  return getFormScreen(ctx).autoRefreshInterval;
}