import {flow} from "mobx";
import {getFavorites} from "model/selectors/MainMenu/getFavorites";
import {handleError} from "model/actions/handleError";

export function runInFlowWithHandler(args:{ctx: any, action: ()=> Promise<any>}) {
  return flow(function* runWithHandler() {
    try {
      yield args.action();
    } catch (e) {
      yield* handleError(args.ctx)(e);
      throw e;
    }
  })();
}