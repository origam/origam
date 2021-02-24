import {getApi} from "model/selectors/getApi";
import {getDataView} from "model/selectors/DataView/getDataView";
import {getTablePanelView} from "model/selectors/TablePanelView/getTablePanelView";
import {getActivePanelView} from "model/selectors/DataView/getActivePanelView";
import {getGroupingConfiguration} from "../../../selectors/TablePanelView/getGroupingConfiguration";
import {aggregationTypeToNumber} from "../../../entities/types/AggregationType";
import { getSessionId } from "model/selectors/getSessionId";
import {getProperties} from "model/selectors/DataView/getProperties";

export function saveColumnConfigurations(ctx: any) {
  return function* saveColumnConfigurations() {
    const dataView = getDataView(ctx);
    const tablePanelView = getTablePanelView(ctx);
    const groupingConfiguration = getGroupingConfiguration(ctx);

    const tableConfiguration = tablePanelView.configurationManager.defaultTableConfiguration;
    if(!tableConfiguration){
      return;
    }

    for (const property of getProperties(ctx)) {
      const columnConfiguration = tableConfiguration.columnConf.find(conf => conf.id === property.id)
      if(columnConfiguration){
        columnConfiguration.width = property.columnWidth;
      }
    }

    yield getApi(ctx).saveObjectConfiguration({
      sessionFormIdentifier: getSessionId(ctx),
      instanceId: dataView.modelInstanceId,
      columnSettings: tableConfiguration.tablePropertyIds
        .map(id=> tableConfiguration.columnConf.find(configuration=> configuration.id === id))
        .filter(configuration => configuration)
        .map(columnConfiguration => {
          return {
            propertyId: columnConfiguration!.id,
            width: columnConfiguration!.width,
            isHidden: !columnConfiguration!.isVisible,
            aggregationTypeNumber: aggregationTypeToNumber(columnConfiguration!.aggregationType),
            groupingIndex: columnConfiguration!.groupingIndex,
            timeGroupingUnit: columnConfiguration!.timeGroupingUnit
        }
      }),
      defaultView: getActivePanelView(ctx),
      lockedColumns: tablePanelView.fixedColumnCount
    });
  };
}
