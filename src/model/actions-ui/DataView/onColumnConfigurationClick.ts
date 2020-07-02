import {getColumnConfigurationDialog} from "model/selectors/getColumnConfigurationDialog";
import {flow} from "mobx";
import {handleError} from "model/actions/handleError";

export function onColumnConfigurationClick(ctx: any) {
  return flow(function* onColumnConfigurationClick(event: any) {
    try {
      getColumnConfigurationDialog(ctx).onColumnConfClick(event);
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  });
}
