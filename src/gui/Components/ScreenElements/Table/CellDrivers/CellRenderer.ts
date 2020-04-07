import { IRowDriver, IDataRow } from "./types";

export class CellRenderer {
  constructor(
    private rowDrivers: IRowDriver[],
    private getRowByIndex: (rowIndex: number) => IDataRow
  ) {}

  renderCell(rowIndex: number, columnIndex: number, ctx: any) {
    const row = this.getRowByIndex(rowIndex);
    for (let driver of this.rowDrivers) driver.render(rowIndex, columnIndex, row, ctx);
  }
}
