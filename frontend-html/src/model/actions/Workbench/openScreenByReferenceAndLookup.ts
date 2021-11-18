import { getApi } from "model/selectors/getApi";
import { getWorkbenchLifecycle } from "model/selectors/getWorkbenchLifecycle";


export function openScreenByReferenceAndLookup(ctx: any) {
  return function* openScreenByReferenceAndLookup(dataSourceLookupId: string, referenceId: string): Generator {
    const api = getApi(ctx);
    const menuId = yield api.getMenuId({
      LookupId: dataSourceLookupId,
      ReferenceId: referenceId,
    });
    yield* getWorkbenchLifecycle(ctx).onMainMenuItemIdClick({
      event: undefined,
      itemId: menuId,
      idParameter: referenceId,
      isSingleRecordEdit: true
    });
  };
}
