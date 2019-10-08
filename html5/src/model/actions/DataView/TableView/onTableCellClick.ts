import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView"

export function onTableCellClick(ctx: any) {
  return function onTableCellClick(rowIndex: number, columnIndex: number) {
    
    getTablePanelView(ctx).onCellClick(rowIndex, columnIndex);
    
  }
}