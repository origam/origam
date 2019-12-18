import { getApi } from "model/selectors/getApi";

export function saveSplitPanelConfiguration(ctx: any) {
  return function* saveSplitPanelConfiguration(
    modelInstanceId: string,
    position: number
  ) {
    yield getApi(ctx).saveSplitPanelConfiguration({
      InstanceId: modelInstanceId,
      Position: position
    });
  };
}
