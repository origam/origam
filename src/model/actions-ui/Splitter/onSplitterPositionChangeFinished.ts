import {flow} from "mobx";
import {saveSplitPanelConfiguration} from "model/actions/FormScreen/saveSplitPanelConfiguration";
import { splitterPositionToRatio } from "./splitterPositionToServerValue";

export function onSplitterPositionChangeFinished(ctx: any) {
  return flow(function* onSplitterPositionChangeFinished(
    modelInstanceId: string,
    position: number
  ) {
    yield* saveSplitPanelConfiguration(ctx)(
      modelInstanceId,
      splitterPositionToRatio(position)
    );
  });
}
