import { getFormScreen } from "./getFormScreen";

export function getIsSuppressSave(ctx: any) {
  return getFormScreen(ctx).suppressSave;
}