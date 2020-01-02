import { flow } from "mobx";
import { stringify } from "querystring";
import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";
import { saveColumnConfigurations } from "model/actions/DataView/TableView/saveColumnConfigurations";

export function onColumnOrderChangeFinished(ctx: any) {
  return flow(function* onColumnOrderChangeFinished(id1: string, id2: string) {
    const tablePanelView = getTablePanelView(ctx);
    tablePanelView.swapColumns(id1, id2);
    yield* saveColumnConfigurations(ctx)();

    // TODO: Error handling
  });
}
