import { flow } from "mobx";
import { getApplicationLifecycle } from "model/selectors/getApplicationLifecycle";

export function onLoginPageSubmitButtonClick(ctx: any) {
  return flow(function* onLoginPageSubmitButtonClick(args: {
    event: any;
    userName: string;
    password: string;
  }) {
    yield* getApplicationLifecycle(ctx).onLoginFormSubmit(args);
  });
}
