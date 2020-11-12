import { flow } from "mobx";
import { onRootElementClick } from "../Global/onRootElementClick";
import { handleError } from "../handleError";

export function onIFrameClick(ctx: any) {
  return flow(function* onIFrameClick(event: any): Generator {
    try {
      yield onRootElementClick(ctx)(event);
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  });
}
