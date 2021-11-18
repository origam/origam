import {getFormScreen} from "./FormScreen/getFormScreen";

export function getSessionId(ctx: any) {
  return getFormScreen(ctx).sessionId;
}