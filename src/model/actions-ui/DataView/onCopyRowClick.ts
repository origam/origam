import { flow } from "mobx";
import { getDataView } from "../../selectors/DataView/getDataView";
import { getGridId } from "../../selectors/DataView/getGridId";
import { getEntity } from "../../selectors/DataView/getEntity";
import { getFormScreenLifecycle } from "../../selectors/FormScreen/getFormScreenLifecycle";
import { handleError } from "../../actions/handleError";


export function onCopyRowClick(ctx: any) {
  return flow(function* onCopyRowClick(event: any) {
    try {
      const selectedRowId = getDataView(ctx).selectedRowId;
      if (!selectedRowId) {
        return;
      }
      const gridId = getGridId(ctx);
      const entity = getEntity(ctx);
      const formScreenLifecycle = getFormScreenLifecycle(ctx);
      yield* formScreenLifecycle.onCopyRow(entity, gridId, selectedRowId);
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  });
}