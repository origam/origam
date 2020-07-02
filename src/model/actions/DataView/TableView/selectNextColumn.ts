import {getTablePanelView} from "model/selectors/TablePanelView/getTablePanelView";

export function selectNextColumn(ctx: any) {
  return function selectNextColumn(nextRowWhenEnd?: boolean) {
    getTablePanelView(ctx).selectNextColumn(nextRowWhenEnd);
  };
}
