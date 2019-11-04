import { getDataView } from "../../selectors/DataView/getDataView";
import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";
import { getEntity } from "../../selectors/DataView/getEntity";
import { getGridId } from "../../selectors/DataView/getGridId";

export function onAddRowClick(ctx: any) {
  return function onAddRowClick() {
    const gridId = getGridId(ctx);
    const entity = getEntity(ctx);
    const formScreenLifecycle = getFormScreenLifecycle(ctx);
    formScreenLifecycle.onCreateRow(entity, gridId);
  };
}
