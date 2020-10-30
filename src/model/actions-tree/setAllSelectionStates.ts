import { flow } from "mobx";
import { getDataTable } from "model/selectors/DataView/getDataTable";
import { getSelectionMember } from "model/selectors/DataView/getSelectionMember";
import { getDataSourceFieldByName } from "model/selectors/DataSources/getDataSourceFieldByName";
import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";
import { setSelectedStateRowId } from "model/actions-tree/selectionCheckboxes";


export function setAllSelectionStates(ctx: any, selectionState: boolean) {
  flow(function* () {
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
  })();
}