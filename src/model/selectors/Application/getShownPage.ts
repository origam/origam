import {getApplicationLifecycle} from "../getApplicationLifecycle";

export function getShownPage(ctx: any) {
  return getApplicationLifecycle(ctx).shownPage;
}
