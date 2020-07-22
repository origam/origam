import {getApi} from "model/selectors/getApi";
import {getDataView} from "model/selectors/DataView/getDataView";
import {getTablePanelView} from "model/selectors/TablePanelView/getTablePanelView";
import {getActivePanelView} from "model/selectors/DataView/getActivePanelView";
import {getGroupingConfiguration} from "../../../selectors/TablePanelView/getGroupingConfiguration";
import {aggregationTypeToNumber} from "../../../entities/types/AggregationType";

export function saveColumnConfigurations(ctx: any) {
  return function* saveColumnConfigurations() {
    const dataView = getDataView(ctx);
    const tablePanelView = getTablePanelView(ctx);
    const groupingConfiguration = getGroupingConfiguration(ctx);

    yield getApi(ctx).saveObjectConfiguration({
      instanceId: dataView.modelInstanceId,
      columnSettings: tablePanelView.allTableProperties.map(property => ({
        propertyId: property.id,
        width: property.columnWidth,
        isHidden: !!tablePanelView.hiddenPropertyIds.get(property.id),
        aggregationTypeNumber: aggregationTypeToNumber(tablePanelView.aggregations.getType(property.id)),
        groupingIndex: groupingConfiguration.groupingIndices.get(property.id)
      })),
      defaultView: getActivePanelView(ctx)
    });
  };
}
