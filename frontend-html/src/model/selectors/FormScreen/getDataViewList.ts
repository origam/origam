import {getFormScreen} from "./getFormScreen";

export function getDataViewList(ctx: any) {
  return getFormScreen(ctx).dataViews;
}
