import {getDataView} from "model/selectors/DataView/getDataView";

export function getConfigurationManager(ctx: any) {
  return getDataView(ctx).tablePanelView.configurationManager;
}