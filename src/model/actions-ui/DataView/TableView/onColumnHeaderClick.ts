import { getOrderingConfiguration } from "model/selectors/DataView/getOrderingConfiguration";
import { runInAction, flow } from "mobx";
import { handleError } from "model/actions/handleError";

export function onColumnHeaderClick(ctx: any) {
  return flow(function* onColumnHeaderClick(event: any, column: string) {
    try {
      if (event.ctrlKey) {
        getOrderingConfiguration(ctx).addOrdering(column);
      } else {
        getOrderingConfiguration(ctx).setOrdering(column);
      }
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  });
}
