import { getApplicationLifecycle } from "model/selectors/getApplicationLifecycle";

export function getLoginPageFormInfoText(ctx: any) {
  return getApplicationLifecycle(ctx).loginPageMessage || ""
}