import {getFormScreen} from "./getFormScreen";

export function getRootDataViews(ctx: any) {
  return getFormScreen(ctx).rootDataViews;
}