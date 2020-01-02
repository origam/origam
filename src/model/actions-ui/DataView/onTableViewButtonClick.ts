import { getDataView } from "model/selectors/DataView/getDataView";
import { flow } from "mobx";
import { handleError } from "model/actions/handleError";
import { saveColumnConfigurations } from "model/actions/DataView/TableView/saveColumnConfigurations";

export function onTableViewButtonClick(ctx: any) {
  return flow(function* onTableViewButtonClick(event: any) {
    try {
      getDataView(ctx).onTablePanelViewButtonClick(event);
      yield* saveColumnConfigurations(ctx)();
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  });
}
