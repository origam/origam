import { getApi } from "model/selectors/getApi";
import { getDataView } from "model/selectors/DataView/getDataView";

export function saveColumnConfigurations(ctx: any) {
  return function* saveColumnConfigurations() {
    const dataView = getDataView(ctx);
    yield getApi(ctx).saveObjectConfiguration({
      instanceId: dataView.modelInstanceId,
      columnSettings: dataView.properties.map(property => ({
        propertyId: property.id,
        width: property.columnWidth
      }))
    });
    yield;
  };
}
