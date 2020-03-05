import { FormScreenLifecycle02 } from "../entities/FormScreenLifecycle/FormScreenLifecycle";
import { FormScreenEnvelope } from "model/entities/FormScreen";
import { IRefreshOnReturnType } from "model/entities/WorkbenchLifecycle/WorkbenchLifecycle";

export function createFormScreenEnvelope(
  preloadedSessionId?: string,
  refreshOnReturnType?: IRefreshOnReturnType
) {
  return new FormScreenEnvelope({
    formScreenLifecycle: new FormScreenLifecycle02(),
    preloadedSessionId,
    refreshOnReturnType
  });
}
