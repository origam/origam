import { FormScreenLifecycle02 } from "../entities/FormScreenLifecycle/FormScreenLifecycle";
import { FormScreenEnvelope } from "model/entities/FormScreen";

export function createFormScreenEnvelope(preloadedSessionId?: string) {
  return new FormScreenEnvelope({
    formScreenLifecycle: new FormScreenLifecycle02(),
    preloadedSessionId
  });
}
