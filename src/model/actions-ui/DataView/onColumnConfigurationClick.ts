import {getColumnConfigurationDialog} from "model/selectors/getColumnConfigurationDialog";
import {flow} from "mobx";
import {handleError} from "model/actions/handleError";
import {shouldProceedToChangeRow} from "./TableView/shouldProceedToChangeRow";

export function onColumnConfigurationClick(ctx: any) {
  return flow(function* onColumnConfigurationClick(event: any) {
    try {
      if(yield shouldProceedToChangeRow(ctx)){
        getColumnConfigurationDialog(ctx).onColumnConfClick(event);
      }
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  });
}
