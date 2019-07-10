import { getDataTable } from "../DataView/getDataTable";
import { getTableViewProperties } from "./getTableViewProperties";
import { getTablePanelView } from "./getTablePanelView";

export function getCellValue(ctx: any) {
  const tablePanelView = getTablePanelView(ctx);
  return function getCellValue(rowIndex: number, columnIndex: number) {
    return tablePanelView.getCellValueByIdx(rowIndex, columnIndex)
  };
}
