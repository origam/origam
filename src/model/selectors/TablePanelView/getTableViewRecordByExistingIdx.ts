import {getDataTable} from "../DataView/getDataTable";

export function getTableViewRecordByExistingIdx(ctx: any, idx: number) {
  return getDataTable(ctx).getRowByExistingIdx(idx);
}
