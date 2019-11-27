import { flow } from "mobx";
import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";

export function onCellClick(ctx: any) {
  return flow(function* onCellClick(event: any, rowIndex: number, columnIndex: number) {
    yield* getTablePanelView(ctx).onCellClick(event, rowIndex, columnIndex);
  })
}