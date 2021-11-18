import { getDataView } from "./getDataView";
import { getRowStateAllowUpdate } from "../RowState/getRowStateAllowUpdate";
import { getSelectedRowId } from "../TablePanelView/getSelectedRowId";

export function getIsMoveRowMenuVisible(ctx: any) {
  const { orderProperty } = getDataView(ctx);
  const currentRowId = getSelectedRowId(ctx);
  return (
    !!orderProperty &&
    !orderProperty?.readOnly &&
    currentRowId &&
    getRowStateAllowUpdate(ctx, currentRowId, orderProperty?.id)
  );
}
