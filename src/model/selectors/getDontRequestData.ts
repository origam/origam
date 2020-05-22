import {getOpenedScreen} from "./getOpenedScreen";

export function getDontRequestData(ctx: any) {
  return getOpenedScreen(ctx).dontRequestData;
}
