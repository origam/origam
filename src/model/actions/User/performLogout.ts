import { getApplicationLifecycle } from "model/selectors/getApplicationLifecycle"

export function performLogout(ctx: any) {
  return function* performLogout() {
    yield* getApplicationLifecycle(ctx).performLogout();
  }
}