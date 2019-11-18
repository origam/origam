import { getGridId } from "model/selectors/DataView/getGridId";
import { getEntity } from "model/selectors/DataView/getEntity";
import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";

export function onCreateRowClick(ctx: any) {
  return function onCreateRowClick(event: any) {
    const gridId = getGridId(ctx);
    const entity = getEntity(ctx);
    const formScreenLifecycle = getFormScreenLifecycle(ctx);
    formScreenLifecycle.onCreateRow(entity, gridId);
  }
}