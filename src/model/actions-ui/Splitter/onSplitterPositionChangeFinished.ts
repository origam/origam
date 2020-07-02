import {flow} from "mobx";
import {saveSplitPanelConfiguration} from "model/actions/FormScreen/saveSplitPanelConfiguration";

export function onSplitterPositionChangeFinished(ctx: any) {
  return flow(function* onSplitterPositionChangeFinished(
    modelInstanceId: string,
    position: number
  ) {
    yield* saveSplitPanelConfiguration(ctx)(
      modelInstanceId,
      Math.round(position)
    );
  });
}
