import { IColumnDriver, IDataRow } from "./types";

export class GroupHeaderColumnDriver implements IColumnDriver {
  constructor(private groupLevel: number) {}

  render(rowIndex: number, columnIndex: number, row: IDataRow, ctx: any): void {}
}
