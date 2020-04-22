export interface IProperty {
  id: string;
  column: string;
  dataIndex: number;
}

export interface IGroupRow {
  groupLevel: number;
  columnLabel: string;
  columnValue: string;
  isExpanded: boolean;
  sourceGroup: IGroupItem;
  parent: IGroupRow | undefined;
}

export interface IGroupItem {
  childGroups: IGroupItem[];
  childRows: any[][];
  columnLabel: string;
  groupLabel: string;
  isExpanded: boolean;
  rowCount: number;
}

export type ITableRow = any[] | IGroupRow;

export interface IClickSubsItem {
  handler(event: any, worldX: number, worldY: number, canvasX: number, canvasY: number): void;
  x: number;
  y: number;
  w: number;
  h: number;
}
