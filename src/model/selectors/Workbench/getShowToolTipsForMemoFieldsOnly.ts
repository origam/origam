import {getWorkbenchLifecycle} from "model/selectors/getWorkbenchLifecycle";

export function getShowToolTipsForMemoFieldsOnly(ctx: any) {
  return getWorkbenchLifecycle(ctx).portalSettings?.showToolTipsForMemoFieldsOnly;
}