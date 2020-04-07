import { IColumnDriver, IDataRow } from "./types";

export class CheckboxColumnDriver implements IColumnDriver {
  constructor() {}

  render(rowIndex: number, columnIndex: number, row: IDataRow, ctx: any): void {}
}