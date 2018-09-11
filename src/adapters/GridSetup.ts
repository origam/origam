import { IGridSetup } from "../Grid/types";
import { decorate, computed } from "mobx";

export class GridSetup implements IGridSetup {
  public get columnCount(): number {
    throw new Error("Method not implemented.");
  } 

  public get fixedColumnCount(): number {
    throw new Error("Method not implemented.");
  }

  public get rowCount(): number {
    throw new Error("Method not implemented.");
  }

  public get isScrollingEnabled(): boolean {
    throw new Error("Method not implemented.");
  }

  public getCellTop(cellIndex: number): number {
    throw new Error("Method not implemented.");
  }

  public getCellLeft(cellIndex: number): number {
    throw new Error("Method not implemented.");
  }

  public getCellBottom(cellIndex: number): number {
    throw new Error("Method not implemented.");
  }

  public getCellRight(cellIndex: number): number {
    throw new Error("Method not implemented.");
  }

  public getCellValue(rowIndex: number, columnIndex: number): string {
    throw new Error("Method not implemented.");
  }

  public getColumnLabel(columnIndex: number): string {
    throw new Error("Method not implemented.");
  }

  public getRowTop(rowIndex: number): number {
    throw new Error("Method not implemented.");
  }

  public getRowBottom(rowIndex: number): number {
    throw new Error("Method not implemented.");
  }
  
  public getRowHeight(rowIndex: number): number {
    throw new Error("Method not implemented.");
  }

  public getColumnLeft(columnIndex: number): number {
    throw new Error("Method not implemented.");
  }

  public getColumnRight(columIndex: number): number {
    throw new Error("Method not implemented.");
  }

  public getColumnWidth(columnIndex: number): number {
    throw new Error("Method not implemented.");
  }
}

decorate(GridSetup, {
  columnCount: computed,
  fixedColumnCount: computed,
  rowCount: computed,
  isScrollingEnabled: computed,

})