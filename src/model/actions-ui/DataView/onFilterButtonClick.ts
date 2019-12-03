import { getFilterConfiguration } from "model/selectors/DataView/getFilterConfiguration";
import { flow } from "mobx";
import { handleError } from "model/actions/handleError";

export function onFilterButtonClick(ctx: any) {
  return flow(function* onFilterButtonClick(event: any) {
    try {
      getFilterConfiguration(ctx).onFilterDisplayClick(event);
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  });
}
