import { flow } from "mobx";
import { onFormTabCloseClick } from "model/actions/onFormTabCloseClick";
import { IOpenedScreen } from "model/entities/types/IOpenedScreen";

export function onScreenTabCloseClick(ctx: any) {
  return flow(function* onScreenTabCloseClick(event: any) {
    yield onFormTabCloseClick(ctx)(event);
  });
}
