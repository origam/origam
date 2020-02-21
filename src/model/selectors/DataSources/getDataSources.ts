import { getFormScreen } from "../FormScreen/getFormScreen";

export function getDataSources(ctx: any) {
  return getFormScreen(ctx).dataSources
}