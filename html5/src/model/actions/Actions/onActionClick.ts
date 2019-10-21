import { getApi } from "model/selectors/getApi";
import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";
import { getGridId } from "model/selectors/DataView/getGridId";
import { getEntity } from "model/selectors/DataView/getEntity";
import { getSelectedRowId } from "model/selectors/TablePanelView/getSelectedRowId";
import { IAction } from "model/entities/types/IAction";
import { flow } from "mobx";
import { IActionMode } from "../../entities/types/IAction";

export function onActionClick(ctx: any) {
  return flow(function*(event: any, action: IAction) {
    if (!action.isEnabled) {
      return;
    }
    const lifecycle = getFormScreenLifecycle(ctx);
    const gridId = getGridId(ctx);
    const entity = getEntity(ctx);
    const rowId = getSelectedRowId(ctx);
    switch (action.mode) {
      case IActionMode.Always:
        yield lifecycle.onExecuteAction(gridId, entity, action, []);
        break;
      case IActionMode.ActiveRecord:
        if (rowId) {
          yield lifecycle.onExecuteAction(gridId, entity, action, [rowId]);
        }
        break;
      case IActionMode.MultipleCheckboxes:
        break;
    }
  });
}
