import {getApplicationLifecycle} from "model/selectors/getApplicationLifecycle";

export function getLoginPageIsWorking(ctx: any) {
  return getApplicationLifecycle(ctx).isWorking;
}
