import { getGridId } from "model/selectors/DataView/getGridId";
import { getEntity } from "model/selectors/DataView/getEntity";
import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";
import { flow } from "mobx";

export function onCreateRowClick(ctx: any) {
  return flow(function* onCreateRowClick(event: any) {
    const gridId = getGridId(ctx);
    const entity = getEntity(ctx);
    const formScreenLifecycle = getFormScreenLifecycle(ctx);
    yield* formScreenLifecycle.onCreateRow(entity, gridId);
  })
}