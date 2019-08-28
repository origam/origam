import { IActionResultType } from "../SelectionDialog/types";
import { getWorkbenchLifecycle } from "../../selectors/getWorkbenchLifecycle";
export function processActionResult(ctx: any) {
  return function processActionResult(actionResultList: any) {
    console.log("actionresult:", actionResultList);
    for (let actionResult of actionResultList)
      switch (actionResult.type) {
        case IActionResultType.OpenForm:
          const { request } = actionResult;
          const { objectId, typeString, dataRequested, parameters } = request;
          const workbenchLifecycle = getWorkbenchLifecycle(ctx);
          // TODO: Check parameter correctness.
          // TODO: Caption / Label priority...?
          workbenchLifecycle.openNewForm(
            objectId,
            typeString,
            "",
            !dataRequested,
            undefined,
            parameters
          );
      }
  };
}
