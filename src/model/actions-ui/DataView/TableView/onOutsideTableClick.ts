import { flow } from "mobx";
import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";
import { handleError } from "model/actions/handleError";

export function onOutsideTableClick(ctx: any) {
  return flow(function* onOutsideTableClick(event: any) {
    try {
      yield* getTablePanelView(ctx).onOutsideTableClick();
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  });
}
