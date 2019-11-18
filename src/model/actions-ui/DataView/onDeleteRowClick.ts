import { getDataView } from "model/selectors/DataView/getDataView";
import { getSelectedRow } from "model/selectors/DataView/getSelectedRow";
import { getDataTable } from "model/selectors/DataView/getDataTable";
import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";

export function onDeleteRowClick(ctx: any) {
  return function onDeleteRowClick(event: any) {
    const dataView = getDataView(ctx);
    const selectedRow = getSelectedRow(ctx);
    if (selectedRow) {
      const dataTable = getDataTable(ctx);
      const entity = dataView.entity;
      const formScreenLifecycle = getFormScreenLifecycle(ctx);
      formScreenLifecycle.onDeleteRow(entity, dataTable.getRowId(selectedRow));
    }
  };
}
