import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";
import { flow } from "mobx";

export function onTableCellClick(ctx: any) {
  return flow(function* onTableCellClick(
    event: any,
    rowIndex: number,
    columnIndex: number
  ) {
    yield* getTablePanelView(ctx).onCellClick(event, rowIndex, columnIndex);
  });
}
