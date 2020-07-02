import {getDataView} from "model/selectors/DataView/getDataView";
import {getSelectedRow} from "model/selectors/DataView/getSelectedRow";
import {getDataTable} from "model/selectors/DataView/getDataTable";
import {getFormScreenLifecycle} from "model/selectors/FormScreen/getFormScreenLifecycle";
import {flow} from "mobx";
import {handleError} from "model/actions/handleError";

export function onDeleteRowClick(ctx: any) {
  return flow(function* onDeleteRowClick(event: any) {
    try {
      const dataView = getDataView(ctx);
      const selectedRow = getSelectedRow(ctx);
      if (selectedRow) {
        const dataTable = getDataTable(ctx);
        const entity = dataView.entity;
        const formScreenLifecycle = getFormScreenLifecycle(ctx);
        yield* formScreenLifecycle.onDeleteRow(
          entity,
          dataTable.getRowId(selectedRow)
        );
      }
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  });
}
