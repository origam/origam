import { getFormScreenLifecycle } from "../../../selectors/FormScreen/getFormScreenLifecycle";
import { getDataTable } from "model/selectors/DataView/getDataTable";
import { getSelectedRow } from "model/selectors/DataView/getSelectedRow";
export function onFieldBlur(ctx: any) {
  return function onFieldBlur(event: any) {
    console.log("ofd");
    getDataTable(ctx).flushFormToTable(getSelectedRow(ctx)!);
    getFormScreenLifecycle(ctx).onFlushData();
  };
}
