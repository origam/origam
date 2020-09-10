import {getSelectedRow} from "model/selectors/DataView/getSelectedRow";
import {getDataTable} from "model/selectors/DataView/getDataTable";
import {getFormScreenLifecycle} from "model/selectors/FormScreen/getFormScreenLifecycle";
import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";

export function flushCurrentRowData(ctx: any) {
  return function* flushCurrentRowData(finishEditing?: boolean) {
    const row = getSelectedRow(ctx);
    if (row) {
      getDataTable(ctx).flushFormToTable(row);
      if(finishEditing) getTablePanelView(ctx).setEditing(false);
      yield* getFormScreenLifecycle(ctx).onFlushData();
    }
  }
}