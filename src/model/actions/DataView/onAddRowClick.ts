import { getDataView } from "../../selectors/DataView/getDataView";
import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";
export function onAddRowClick(ctx: any) {
  return function onAddRowClick() {
    const dataView = getDataView(ctx);
    const gridId = dataView.modelInstanceId;
    const entity = dataView.entity;
    const formScreenLifecycle = getFormScreenLifecycle(ctx);
    formScreenLifecycle.onCreateRow(entity, gridId);
  };
}
