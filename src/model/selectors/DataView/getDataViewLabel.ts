import {getDataView} from "./getDataView";
import {getConfigurationManager} from "model/selectors/TablePanelView/getConfigurationManager";

export function getDataViewLabel(ctx: any) {
  const activeConfigName = getConfigurationManager(ctx).activeTableConfiguration.name ;
  return activeConfigName
    ? `${getDataView(ctx).name} [${activeConfigName}]`
    : getDataView(ctx).name
}