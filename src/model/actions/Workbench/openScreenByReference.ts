import { getApi } from "model/selectors/getApi";
import { getWorkbenchLifecycle } from "model/selectors/getWorkbenchLifecycle";

export function openScreenByReference(ctx: any) {
  return function* openScreenByReference(lookupId: string, referenceId: string): Generator {
    const api = getApi(ctx);
    const menuId = yield api.getMenuId({
      LookupId: lookupId,
      ReferenceId: referenceId,
    });
    yield* getWorkbenchLifecycle(ctx).onMainMenuItemIdClick({
      event: undefined,
      itemId: menuId,
      idParameter: referenceId,
    });
  };
}
