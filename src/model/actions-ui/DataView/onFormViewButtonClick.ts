import { getDataView } from "model/selectors/DataView/getDataView";
import { flow } from "mobx";
import { handleError } from "model/actions/handleError";

export function onFormViewButtonClick(ctx: any) {
  return flow(function* onFormViewButtonClick(event: any) {
    try {
      getDataView(ctx).onFormPanelViewButtonClick(event);
    } catch (e) {
      handleError(ctx)(e);
      throw e;
    }
  });
}
