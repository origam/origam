import {getDataView} from "model/selectors/DataView/getDataView";

export function getIsRowMovingDisabled(ctx: any) {
  return getDataView(ctx).dataTable.addedRowPositionLocked;
}