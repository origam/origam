import { flow } from "mobx";
import { IAction, IActionMode } from "model/entities/types/IAction";
import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";
import { getGridId } from "model/selectors/DataView/getGridId";
import { getEntity } from "model/selectors/DataView/getEntity";
import { getSelectedRowId } from "model/selectors/TablePanelView/getSelectedRowId";
import { handleError } from "model/actions/handleError";

import selectors from "model/selectors-tree";

export default {
  onActionClick(ctx: any) {
    return flow(function* onActionClick(event: any, action: IAction) {
      try {
        if (!action.isEnabled) {
          return;
        }
        const lifecycle = getFormScreenLifecycle(ctx);
        const gridId = getGridId(ctx);
        const entity = getEntity(ctx);
        const rowId = getSelectedRowId(ctx);
        switch (action.mode) {
          case IActionMode.Always:
            yield* lifecycle.onExecuteAction(gridId, entity, action, []);
            break;
          case IActionMode.ActiveRecord:
            if (rowId) {
              yield* lifecycle.onExecuteAction(gridId, entity, action, [rowId]);
            }
            break;
          case IActionMode.MultipleCheckboxes:
            const selectedRowIds = selectors.selectionCheckboxes.getSelectedRowIds(
              ctx
            );
            yield* lifecycle.onExecuteAction(gridId, entity, action, selectedRowIds);
            break;
        }
      } catch (e) {
        yield* handleError(ctx)(e);
        throw e;
      }
    });
  }
};
