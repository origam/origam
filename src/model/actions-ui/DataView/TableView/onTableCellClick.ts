import { flow } from "mobx";
import { handleError } from "model/actions/handleError";
import { getDataSourceFieldByName } from "model/selectors/DataSources/getDataSourceFieldByName";
import { getDataTable } from "model/selectors/DataView/getDataTable";
import { getIsSelectionCheckboxesShown } from "model/selectors/DataView/getIsSelectionCheckboxesShown";
import { getSelectionMember } from "model/selectors/DataView/getSelectionMember";
import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";
import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";

export function onTableCellClick(ctx: any) {
  return flow(function* onTableCellClick(
    event: any,
    rowIndex: number,
    columnIndex: number
  ) {
    try {
      if (getIsSelectionCheckboxesShown(ctx) && columnIndex === -1) {
        const dataTable = getDataTable(ctx);
        const selectionMember = getSelectionMember(ctx);
        const row = dataTable.getRowByExistingIdx(rowIndex);
        if (!!selectionMember) {
          const dsField = getDataSourceFieldByName(ctx, selectionMember);
          if (dsField) {
            const value = dataTable.getCellValueByDataSourceField(row, dsField);
            dataTable.setDirtyValue(row, selectionMember, !value);
            yield* getFormScreenLifecycle(ctx).onFlushData();
          }
        }
        return;
      }
      yield* getTablePanelView(ctx).onCellClick(event, rowIndex, columnIndex);
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  });
}
