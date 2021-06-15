import {ISortingConfig} from "../../entities/types/ISortingConfig";
import {getWorkbenchLifecycle} from "../getWorkbenchLifecycle";

export function getSortingConfig(ctx: any): ISortingConfig {
  return getWorkbenchLifecycle(ctx).portalSettings?.sortingConfig ?? {
    caseSensitive: false,
    accentSensitive: true
  };
}