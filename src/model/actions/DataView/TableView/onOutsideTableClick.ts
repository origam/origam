import { flow } from "mobx";
import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";

export function onOutsideTableClick(ctx: any) {
  return flow(function* onOutsideTableClick(event: any) {
    yield* getTablePanelView(ctx).onOutsideTableClick();
  })
}