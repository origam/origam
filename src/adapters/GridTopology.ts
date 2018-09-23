import { IGridTopology } from "../Grid/types";

export class GridTopology implements IGridTopology {
  public getUpRowId(rowId: string): string {
    throw new Error("Method not implemented.");
  }

  public getDownRowId(rowId: string): string {
    throw new Error("Method not implemented.");
  }

  public getLeftColumnId(columnId: string): string {
    throw new Error("Method not implemented.");
  }

  public getRightColumnId(columnId: string): string {
    throw new Error("Method not implemented.");
  }

  public getColumnIdByIndex(columnIndex: number): string {
    return columnIndex+'';
  }

  public getRowIdByIndex(rowIndex: number): string {
    return rowIndex+'';
  }

  public getColumnIndexById(columnId: string): number {
    return parseInt(columnId, 10);
  }

  public getRowIndexById(rowId: string): number {
    return parseInt(rowId, 10);
  }


}