import { flow } from "mobx";
import { saveSplitPanelConfiguration } from "model/actions/FormScreen/saveSplitPanelConfiguration";
import { getFormScreen } from "model/selectors/FormScreen/getFormScreen";

export function onSplitterPositionChangeFinished(ctx: any) {
  return flow(function* onSplitterPositionChangeFinished(
    modelInstanceId: string,
    position: number
  ) {
    getFormScreen(ctx).setPanelSize(modelInstanceId, position);
    yield* saveSplitPanelConfiguration(ctx)(modelInstanceId, Math.round(position * 1e6)); //splitterPositionToRatio(position));
  });
}
