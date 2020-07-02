import {flow} from "mobx";
import {getTablePanelView} from "model/selectors/TablePanelView/getTablePanelView";
import {handleError} from "model/actions/handleError";

export function onNoCellClick(ctx: any) {
  return flow(function* onNoCellClick(event: any) {
    try {
      yield* getTablePanelView(ctx).onNoCellClick();
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  });
}
