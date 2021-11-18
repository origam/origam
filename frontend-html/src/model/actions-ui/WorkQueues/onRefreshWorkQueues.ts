import {flow} from "mobx";
import {refreshWorkQueues} from "model/actions/WorkQueues/refreshWorkQueues";
import {handleError} from "model/actions/handleError";

export function onRefreshWorkQueues(ctx: any) {
  return function* onRefreshWorkQueues() {
    try {
      yield* refreshWorkQueues(ctx)();
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  };
}
