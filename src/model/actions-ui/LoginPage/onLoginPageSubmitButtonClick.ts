import {flow} from "mobx";
import {getApplicationLifecycle} from "model/selectors/getApplicationLifecycle";
import {handleError} from "model/actions/handleError";

export function onLoginPageSubmitButtonClick(ctx: any) {
  return flow(function* onLoginPageSubmitButtonClick(args: {
    event: any;
    userName: string;
    password: string;
  }) {
    try {
      yield* getApplicationLifecycle(ctx).onLoginFormSubmit(args);
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  });
}
