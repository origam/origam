import {flow} from "mobx";
import {getDataViewPropertyById} from "model/selectors/DataView/getDataViewPropertyById";
import {saveColumnConfigurations} from "model/actions/DataView/TableView/saveColumnConfigurations";
import {getTablePanelView} from "model/selectors/TablePanelView/getTablePanelView";

export function onColumnWidthChangeFinished(ctx: any) {
  return flow(function* onColumnWidthChangeFinished(id: string, width: number) {
    // TODO: Error handling
    const prop = getDataViewPropertyById(ctx, id);
    if(prop) {
      const columnConfiguration = getTablePanelView(ctx).configurationManager.defaultTableConfiguration.columnConfiguration
        .find(configuration =>  configuration.propertyId === prop.id);
      if(columnConfiguration){
        columnConfiguration.width = width;
      }
      yield* saveColumnConfigurations(ctx)();

      // TODO: Error handling
    }
  });
}