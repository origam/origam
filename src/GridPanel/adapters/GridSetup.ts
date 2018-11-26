import { action, computed, observable } from "mobx";
import {
  IGridProps,
  IGridSetup,
  IGridInteractionSelectors
} from "../../Grid/types";
import { IDataTableSelectors, ICellValue } from "../../DataTable/types";

export class GridSetup implements IGridSetup {
  constructor(
    public gridInteractionSelectors: IGridInteractionSelectors,
    public dataTableSelectors: IDataTableSelectors
  ) {}

  @computed
  public get columnCount(): number {
    return this.dataTableSelectors.fieldCount;
  }

  @computed
  public get fixedColumnCount(): number {
    return 0;
  }

  @computed
  public get rowCount(): number {
    return this.dataTableSelectors.recordCount;
  }

  @computed
  public get isScrollingEnabled(): boolean {
    return !this.gridInteractionSelectors.isCellEditing;
  }

  public isFixedColumn(columnIndex: number): boolean {
    return columnIndex < this.fixedColumnCount;
  }

  public getCellTop(cellIndex: number): number {
    return cellIndex * this.getRowHeight(cellIndex);
  }

  public getCellLeft(cellIndex: number): number {
    if(cellIndex === 0) {
      return 0;
    } else {
      return this.getCellRight(cellIndex - 1);
    }
  }

  public getCellBottom(cellIndex: number): number {
    return this.getCellTop(cellIndex) + this.getRowHeight(cellIndex);
  }

  public getCellRight(cellIndex: number): number {
    return this.getCellLeft(cellIndex) + this.getColumnWidth(cellIndex);
  }

  public getCellValue(
    rowIndex: number,
    columnIndex: number
  ): ICellValue | undefined {
    const record = this.dataTableSelectors.getRecordByRecordIndex(rowIndex);
    const field = this.dataTableSelectors.getFieldByFieldIndex(columnIndex);
    if (record && field) {
      return this.dataTableSelectors.getValue(record, field);
    } else {
      return
    }
  }

  public getColumnLabel(columnIndex: number): string {
    const field = this.dataTableSelectors.getFieldByFieldIndex(columnIndex);
    return field ? field.label : `Field ${field}`;
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
    const field = this.dataTableSelectors.getFieldByFieldIndex(columnIndex);
    if(field) {
      const width = this.gridInteractionSelectors.getColumnWidth(field.id);
      return width;
    } else {
      return 100;
    }
  }

  public onRowsRendered(rowIndexStart: number, rowIndexEnd: number): void {
    return;
  }
}
