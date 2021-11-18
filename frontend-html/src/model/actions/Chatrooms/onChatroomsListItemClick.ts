import {flow} from "mobx";
import {handleError} from "model/actions/handleError";
import {getWorkbenchLifecycle} from "model/selectors/getWorkbenchLifecycle";

export function onChatroomsListItemClick(ctx: any) {
  return flow(function* onChatroomsListItemClick(event: any, item: any) {
    try {
      yield* getWorkbenchLifecycle(ctx).onChatroomsListItemClick(event, item);
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  });
}
