import {flow} from "mobx";
import {handleError} from "model/actions/handleError";

export function runInFlowWithHandler(args:{ctx: any, action: (()=> Promise<any>) | (()=> void)}) {
  return flow(function* runWithHandler() {
    try {
      yield args.action();
    } catch (e) {
      yield* handleError(args.ctx)(e);
      throw e;
    }
  })();
}