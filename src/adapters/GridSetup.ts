import { IGridSetup } from "../Grid/types";
import { decorate, computed } from "mobx";

export class GridSetup implements IGridSetup {
  public get columnCount(): number {
    return 50;
  }

  public get fixedColumnCount(): number {
    return 3;
  }

  public get rowCount(): number {
    return 100;
  }

  public get isScrollingEnabled(): boolean {
    return true;
  }

  public getCellTop(cellIndex: number): number {
    return cellIndex * 20;
  }

  public getCellLeft(cellIndex: number): number {
    return cellIndex * 100;
  }

  public getCellBottom(cellIndex: number): number {
    return this.getCellTop(cellIndex) + 20;
  }

  public getCellRight(cellIndex: number): number {
    return this.getCellLeft(cellIndex) + 100;
  }

  public getCellValue(rowIndex: number, columnIndex: number): string {
    return `${rowIndex};${columnIndex}`
  }

  public getColumnLabel(columnIndex: number): string {
    return `${columnIndex}`
  }

  public getRowTop(rowIndex: number): number {
    return this.getCellTop(rowIndex);
  }

  public getRowBottom(rowIndex: number): number {
    return this.getCellBottom(rowIndex);
  }

  public getRowHeight(rowIndex: number): number {
    return 20;
  }

  public getColumnLeft(columnIndex: number): number {
    return this.getCellLeft(columnIndex);
  }

  public getColumnRight(columIndex: number): number {
    return this.getCellRight(columIndex);
  }

  public getColumnWidth(columnIndex: number): number {
    return 100;
  }

  public onRowsRendered(rowIndexStart: number, rowIndexEnd: number): void {
    return
  }
}

decorate(GridSetup, {
  columnCount: computed,
  fixedColumnCount: computed,
  rowCount: computed,
  isScrollingEnabled: computed
});
