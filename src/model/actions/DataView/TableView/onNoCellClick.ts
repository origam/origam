import { flow } from "mobx";
import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";

export function onNoCellClick(ctx: any) {
  return flow(function* onNoCellClick(event: any) {
    yield* getTablePanelView(ctx).onNoCellClick();
  })
}