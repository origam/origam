import { getDataView } from "model/selectors/DataView/getDataView";

export function onFormViewButtonClick(ctx: any) {
  return function onFormViewButtonClick(event: any) {
    getDataView(ctx).onFormPanelViewButtonClick(event);
  };
}
