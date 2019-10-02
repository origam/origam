import { getTablePanelView } from "../../../selectors/TablePanelView/getTablePanelView";

export function selectPrevColumn(ctx: any) {
  return function selectPrevColumn(prevRowWhenStart?: boolean) {
    getTablePanelView(ctx).selectPrevColumn(prevRowWhenStart);
  };
}
