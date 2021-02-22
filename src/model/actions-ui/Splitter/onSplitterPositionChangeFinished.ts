import { flow } from "mobx";
import { saveSplitPanelConfiguration } from "model/actions/FormScreen/saveSplitPanelConfiguration";
import { getFormScreen } from "model/selectors/FormScreen/getFormScreen";
import { splitterPositionToRatio } from "./splitterPositionToServerValue";

export function onSplitterPositionChangeFinished(ctx: any) {
  return flow(function* onSplitterPositionChangeFinished(
    modelInstanceId: string,
    position: number
  ) {
    getFormScreen(ctx).setPanelSize(modelInstanceId, position);
    yield* saveSplitPanelConfiguration(ctx)(modelInstanceId, splitterPositionToRatio(position));
  });
}
