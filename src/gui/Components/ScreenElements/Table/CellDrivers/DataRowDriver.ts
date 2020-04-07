import { IRowDriver, IColumnDriver, IDataRow, isDataRow } from "./types";

export class DataRowDriver implements IRowDriver {
  constructor(private getColumnDrivers: () => IColumnDriver[]) {}

  render(rowIndex: number, columnIndex: number, row: IDataRow, ctx: any): void {
    if (isDataRow(row)) {
      for (let driver of this.getColumnDrivers()) driver.render(rowIndex, columnIndex, row, ctx);
    }
  }
}