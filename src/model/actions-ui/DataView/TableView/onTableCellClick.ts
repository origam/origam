import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";
import { flow } from "mobx";
import { handleError } from "model/actions/handleError";

export function onTableCellClick(ctx: any) {
  return flow(function* onTableCellClick(
    event: any,
    rowIndex: number,
    columnIndex: number
  ) {
    try {
      yield* getTablePanelView(ctx).onCellClick(event, rowIndex, columnIndex);
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  });
}
