import {getGridId} from "model/selectors/DataView/getGridId";
import {getEntity} from "model/selectors/DataView/getEntity";
import {getFormScreenLifecycle} from "model/selectors/FormScreen/getFormScreenLifecycle";
import {flow} from "mobx";
import {handleError} from "model/actions/handleError";
import {getDataView} from "../../selectors/DataView/getDataView";

export function onCreateRowClick(ctx: any) {
  return flow(function* onCreateRowClick(event: any) {
    try {
      const gridId = getGridId(ctx);
      const entity = getEntity(ctx);
      const formScreenLifecycle = getFormScreenLifecycle(ctx);
      yield* formScreenLifecycle.onCreateRow(entity, gridId);
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  });
}

export function onCopyRowClick(ctx: any) {
  return flow(function* onCopyRowClick(event: any) {
    try {
      const selectedRowId = getDataView(ctx).selectedRowId;
      if(!selectedRowId){
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
