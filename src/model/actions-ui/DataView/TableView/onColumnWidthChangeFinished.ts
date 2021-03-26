import {flow} from "mobx";
import {getDataViewPropertyById} from "model/selectors/DataView/getDataViewPropertyById";
import {getTablePanelView} from "model/selectors/TablePanelView/getTablePanelView";

export function onColumnWidthChangeFinished(ctx: any) {
  return flow(function* onColumnWidthChangeFinished(id: string, width: number) {
    // TODO: Error handling
    const prop = getDataViewPropertyById(ctx, id);
    if(prop) {
      yield* getTablePanelView(ctx).configurationManager.onColumnWidthChanged(id, width);

      // TODO: Error handling
    }
  });
}