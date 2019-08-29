import { IActionResultType } from "../SelectionDialog/types";
import { getWorkbenchLifecycle } from "../../selectors/getWorkbenchLifecycle";
import { DialogInfo } from "model/entities/OpenedScreen";
export function processActionResult(ctx: any) {
  return function processActionResult(actionResultList: any) {
    console.log("actionresult:", actionResultList);
    for (let actionResult of actionResultList)
      switch (actionResult.type) {
        case IActionResultType.OpenForm:
          const { request } = actionResult;
          const {
            objectId,
            typeString,
            dataRequested,
            parameters,
            isModalDialog,
            dialogWidth,
            dialogHeight
          } = request;
          const workbenchLifecycle = getWorkbenchLifecycle(ctx);
          // TODO: Check parameter correctness.
          // TODO: Caption / Label priority...?
          const dialogInfo = isModalDialog
            ? new DialogInfo(dialogWidth, dialogHeight)
            : undefined;
          workbenchLifecycle.openNewForm(
            objectId,
            typeString,
            "",
            !dataRequested,
            dialogInfo,
            parameters
          );
      }
  };
}
