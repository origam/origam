import { getApi } from "model/selectors/getApi";
import { getDataView } from "model/selectors/DataView/getDataView";
import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";

export function saveColumnConfigurations(ctx: any) {
  return function* saveColumnConfigurations() {
    const dataView = getDataView(ctx);
    const tablePanelView = getTablePanelView(ctx);
    yield getApi(ctx).saveObjectConfiguration({
      instanceId: dataView.modelInstanceId,
      columnSettings: dataView.properties.map(property => ({
        propertyId: property.id,
        width: property.columnWidth,
        isHidden: !!tablePanelView.hiddenPropertyIds.get(property.id)
      }))
    });
    yield;
  };
}
