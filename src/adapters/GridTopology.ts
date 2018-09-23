import { IGridTopology } from "../Grid/types";

export class GridTopology implements IGridTopology {
  public getUpRowId(rowId: string): string {
    return `${parseInt(rowId, 10) - 1}`;
  }

  public getDownRowId(rowId: string): string {
    return `${parseInt(rowId, 10) + 1}`;
  }

  public getLeftColumnId(columnId: string): string {
    return `${parseInt(columnId, 10) - 1}`;
  }

  public getRightColumnId(columnId: string): string {
    return `${parseInt(columnId, 10) + 1}`;
  }

  public getColumnIdByIndex(columnIndex: number): string {
    return columnIndex + "";
  }

  public getRowIdByIndex(rowIndex: number): string {
    return rowIndex + "";
  }

  public getColumnIndexById(columnId: string): number {
    return parseInt(columnId, 10);
  }

  public getRowIndexById(rowId: string): number {
    return parseInt(rowId, 10);
  }
}
