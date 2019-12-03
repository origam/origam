import { flow } from "mobx";
import { IAction } from "model/entities/types/IAction";
import { getEntity } from "model/selectors/DataView/getEntity";
import { getGridId } from "model/selectors/DataView/getGridId";
import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";
import { getSelectedRowId } from "../../selectors/TablePanelView/getSelectedRowId";
import { handleError } from "model/actions/handleError";

let isRunning = false;

export function onSelectionDialogActionButtonClick(ctx: any) {
  return flow(function*(event: any, action: IAction) {
    try {
      // TODO: Block "re-submission" for all ui actions
      if (isRunning) return;
      try {
        isRunning = true;

        // TODO: Wait for other async operations to finish successfully

        const lifecycle = getFormScreenLifecycle(ctx);
        const gridId = getGridId(ctx);
        const entity = getEntity(ctx);
        const rowId = getSelectedRowId(ctx);
        if (rowId) {
          const selectedItems: string[] = [rowId];
          yield* lifecycle.onExecuteAction(
            gridId,
            entity,
            action,
            selectedItems
          );
          // closeForm(ctx)();
        }
      } finally {
        isRunning = false;
      }
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  });
}
