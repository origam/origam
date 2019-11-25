import { getSelectedRow } from "model/selectors/DataView/getSelectedRow";
import { getDataTable } from "model/selectors/DataView/getDataTable";
import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";

export function flushCurrentRowData(ctx: any) {
  return function* flushCurrentRowData() {
    console.log("ofd");
    const row = getSelectedRow(ctx);
    if (row) {
      getDataTable(ctx).flushFormToTable(row);
      yield* getFormScreenLifecycle(ctx).onFlushData();
    }
  }
}