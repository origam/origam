import { IColumnDriver, IDataRow } from "./types";

export class NoopColumnDriver implements IColumnDriver {
  constructor() {}

  render(rowIndex: number, columnIndex: number, row: IDataRow, ctx: any): void {}
}
