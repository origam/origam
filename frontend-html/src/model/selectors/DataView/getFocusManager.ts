import { FocusManager } from "model/entities/FocusManager";
import { getDataView } from "model/selectors/DataView/getDataView";

export function getFocusManager(ctx: any): FocusManager {
  return getDataView(ctx).focusManager;
}