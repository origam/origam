import { getDataTable } from "../DataView/getDataTable";
import { getTableViewProperties } from "./getTableViewProperties";
import { getTablePanelView } from "./getTablePanelView";
import { IProperty } from "../../entities/types/IProperty";

export function getCellTextByIdx(ctx: any) {
  const tablePanelView = getTablePanelView(ctx);
  return function getCellTextByIdx(rowIndex: number, columnIndex: number) {
    return tablePanelView.getCellTextByIdx(rowIndex, columnIndex);
  };
}

export function getCellText(ctx: any, row: any[], property: IProperty) {
  return getDataTable(ctx).getCellText(row, property);
}
