import { IRowDriver, IColumnDriver, IDataRow, isGroupHeaderRow } from "./types";

export class GroupHeaderRowDriver implements IRowDriver {
  constructor(private getColumnDrivers: () => IColumnDriver[]) {}

  render(rowIndex: number, columnIndex: number, row: IDataRow, ctx: any): void {
    if (isGroupHeaderRow(row)) {
      for (let driver of this.getColumnDrivers()) driver.render(rowIndex, columnIndex, row, ctx);
    }
  }
}