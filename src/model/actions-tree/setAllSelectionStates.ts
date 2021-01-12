import { flow } from "mobx";
import { getDataTable } from "model/selectors/DataView/getDataTable";
import { getSelectionMember } from "model/selectors/DataView/getSelectionMember";
import { getDataSourceFieldByName } from "model/selectors/DataSources/getDataSourceFieldByName";
import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";
import { setSelectedStateRowId } from "model/actions-tree/selectionCheckboxes";
import { handleError } from "model/actions/handleError";
import { getRowStates } from "model/selectors/RowState/getRowStates";
import { getDataView } from "model/selectors/DataView/getDataView";

export function setAllSelectionStates(ctx: any, selectionState: boolean) {
  flow(function* () {
    try {
      yield updateRowStates(ctx);
      const dataTable = getDataTable(ctx);
      const selectionMember = getSelectionMember(ctx);
      if (!!selectionMember) {
        for (let row of dataTable.rows) {
          const dsField = getDataSourceFieldByName(ctx, selectionMember);
          if (dsField) {
            dataTable.setDirtyValue(row, selectionMember, selectionState);
          }
        }
        yield* getFormScreenLifecycle(ctx).onFlushData();
        for (let row of dataTable.rows) {
          const dataSourceField = getDataSourceFieldByName(ctx, selectionMember)!;
          const newSelectionState = dataTable.getCellValueByDataSourceField(row, dataSourceField);
          const rowId = dataTable.getRowId(row);
          yield* setSelectedStateRowId(ctx)(rowId, newSelectionState);
        }
      } else {
        for (let row of dataTable.rows) {
          const rowId = dataTable.getRowId(row);
          yield* setSelectedStateRowId(ctx)(rowId, selectionState);
        }
      }
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    } 
  })();
}

async function updateRowStates(ctx: any){
  const dataView = getDataView(ctx);
  const rowIds = dataView.dataTable.rows.map(row => dataView.dataTable.getRowId(row));
  await getRowStates(ctx).loadValues(rowIds);
}