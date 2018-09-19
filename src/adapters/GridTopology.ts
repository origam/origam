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
    throw new Error("Method not implemented.");
  }

  public getRowIdByIndex(rowIndex: number): string {
    throw new Error("Method not implemented.");
  }

  public getColumnIndexById(columnId: string): number {
    throw new Error("Method not implemented.");
  }

  public getRowIndexById(rowId: string): number {
    throw new Error("Method not implemented.");
  }


}