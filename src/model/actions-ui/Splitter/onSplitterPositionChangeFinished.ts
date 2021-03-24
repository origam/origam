import { flow } from "mobx";
import { saveSplitPanelConfiguration } from "model/actions/FormScreen/saveSplitPanelConfiguration";
import { getFormScreen } from "model/selectors/FormScreen/getFormScreen";
import {panelSizeRatioToServerValue} from "./splitterPositionToServerValue";

export function onSplitterPositionChangeFinished(ctx: any) {
  return flow(function* onSplitterPositionChangeFinished(
    modelInstanceId: string,
    sizeRatio: number
  ) {
    let panelSizeRatioServerValue = panelSizeRatioToServerValue(sizeRatio);
    getFormScreen(ctx).setPanelSize(modelInstanceId, panelSizeRatioServerValue);
    yield* saveSplitPanelConfiguration(ctx)(modelInstanceId, panelSizeRatioServerValue);
  });
}