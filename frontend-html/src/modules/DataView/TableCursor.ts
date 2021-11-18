import { computed } from "mobx";
import { TypeSymbol } from "dic/Container";

export const IRowCursor = TypeSymbol<RowCursor>("IRowCursor");
export class RowCursor {
  constructor(private getSelectedRowId: () => string | undefined) {}

  @computed get selectedId(): string | undefined {
    return this.getSelectedRowId();
  }
}

export const IColumnCursor = TypeSymbol<ColumnCursor>("IColumnCursor");
export class ColumnCursor {
  constructor(private getSelectedColumnId: () => string | undefined) {}

  @computed get selectedId(): string | undefined {
    return this.getSelectedColumnId();
  }
}
