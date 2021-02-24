import {getApi} from "model/selectors/getApi";
import {getDataView} from "model/selectors/DataView/getDataView";
import {getTablePanelView} from "model/selectors/TablePanelView/getTablePanelView";
import {getActivePanelView} from "model/selectors/DataView/getActivePanelView";
import {getGroupingConfiguration} from "../../../selectors/TablePanelView/getGroupingConfiguration";
import {aggregationTypeToNumber} from "../../../entities/types/AggregationType";
import { getSessionId } from "model/selectors/getSessionId";
import {getProperties} from "model/selectors/DataView/getProperties";
import { getConfigurationManager } from "model/selectors/TablePanelView/getConfigurationManager";

export function saveColumnConfigurations(ctx: any) {
  return function* saveColumnConfigurations() {
    const dataView = getDataView(ctx);
    const configurationManager = getConfigurationManager(ctx);
    const groupingConfiguration = getGroupingConfiguration(ctx);

    if(configurationManager.allTableConfigurations.length === 0){
      return;
    }

    const tableConfiguration = configurationManager.defaultTableConfiguration;
    for (const property of getProperties(ctx)) {
      const columnConfiguration = tableConfiguration.columnConf.find(conf => conf.id === property.id)
      if(columnConfiguration){
        columnConfiguration.width = property.columnWidth;
      }
    }

    yield getApi(ctx).saveObjectConfiguration({
      sessionFormIdentifier: getSessionId(ctx),
      instanceId: dataView.modelInstanceId,
      tableConfigurations: configurationManager.allTableConfigurations,
      defaultView: getActivePanelView(ctx),
    });
  };
}
