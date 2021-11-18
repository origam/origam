import {getRowStates} from "./getRowStates";
import {getDataView} from "../DataView/getDataView";

export function getRowStateMayCauseFlicker(ctx: any) {
  const dataView = getDataView(ctx);
  return getRowStates(ctx).mayCauseFlicker && !dataView.dataTable.isEmpty;
}