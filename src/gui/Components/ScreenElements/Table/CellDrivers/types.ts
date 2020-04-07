export interface IGroupHeader {
  columnId: string;
  columnName: string;
  value: string;
  isExpanded: boolean;
  childGroups: IGroupHeader[];
  childRows: any[][];
}

export const isDataRow = (o: any): o is any[] => _.isArray(o);
export const isGroupHeaderRow = (o: any): o is IGroupHeader => !!o.childGroups;

export type IDataRow = any[] | IGroupHeader;

export interface IRowDriver {
  render(rowIndex: number, columnIndex: number, row: IDataRow, ctx: any): void;
}

export interface IColumnDriver {
  render(rowIndex: number, columnIndex: number, row: IDataRow, ctx: any): void;
}

export interface ITableViewInfo {
  isCheckboxes: boolean;
  isGrouping: boolean;
  groupingColumnsCount: number;
  dataColumnsCount: number;
  getGroupedColumnIdByLevel(level: number): string;
  getDataColumnIdByIndex(index: number): string;
}

export interface IColumnDriverFactories {
  newNoopColumnDriver(): IColumnDriver;
  newGroupHeaderColumnDriver(columnId: string): IColumnDriver;
  newCheckboxColumnDriver(): IColumnDriver;
  newDataColumnDriver(columnId: string): IColumnDriver;
}
