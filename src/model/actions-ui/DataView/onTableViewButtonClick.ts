import { getDataView } from "model/selectors/DataView/getDataView";

export function onTableViewButtonClick(ctx: any) {
  return function onTableViewButtonClick(event: any) {
    getDataView(ctx).onTablePanelViewButtonClick(event);
  };
}
