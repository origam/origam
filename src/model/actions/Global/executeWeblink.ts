import { getWorkbenchLifecycle } from "model/selectors/getWorkbenchLifecycle";
import { openScreenByReference } from "../Workbench/openScreenByReference";

export function executeWeblink(ctx: any) {
  return function* executeWeblink(urlPath: string, urlQuery?: { [key: string]: any }): Generator {
    switch (urlPath) {
      case "openScreenByMenuItemId":
        {
          const menuId = urlQuery?.menuItemId;
          if(menuId) {
            yield* getWorkbenchLifecycle(ctx).onMainMenuItemIdClick({
              event: undefined,
              itemId: menuId,
              idParameter: undefined
            });
          }
        }
        break;
      case "objectTag":
        {
          const referenceId = urlQuery?.objectId;
          const lookupId = urlQuery?.categoryId;
          if (referenceId && lookupId) {
            yield* openScreenByReference(ctx)(lookupId, referenceId);
          }
        }
        break;
    }
  };
}
