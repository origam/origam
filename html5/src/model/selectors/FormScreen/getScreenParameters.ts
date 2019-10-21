import { getFormScreen } from "./getFormScreen";
import { getOpenedScreen } from "../getOpenedScreen";

export function getScreenParameters(ctx: any) {
  return getOpenedScreen(ctx).parameters;
}
