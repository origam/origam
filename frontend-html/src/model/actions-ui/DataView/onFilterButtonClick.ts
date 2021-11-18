import {getFilterConfiguration} from "model/selectors/DataView/getFilterConfiguration";
import {flow} from "mobx";
import {handleError} from "model/actions/handleError";
import { getDataView } from "model/selectors/DataView/getDataView";

export function onFilterButtonClick(ctx: any) {
  return flow(function* onFilterButtonClick(event: any) {
    try {
      const dataView = getDataView(ctx);
      if(dataView.isFormViewActive()){
        dataView.activateTableView?.();
      }
      getFilterConfiguration(ctx).onFilterDisplayClick(event);
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  });
}
