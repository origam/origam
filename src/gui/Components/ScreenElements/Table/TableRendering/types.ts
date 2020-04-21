export interface IProperty {
  id: string;
  type: string;
  dataIndex: number;
}

export interface IGroupRow {
  groupLevel: number;
  columnLabel: string;
  columnValue: string;
  isExpanded: boolean;
  sourceGroup: IGroupItem;
}

export interface IGroupItem {
  childGroups: IGroupItem[];
  childRows: any[][];
  columnLabel: string;
  groupLabel: string;
  isExpanded: boolean;
}

export type ITableRow = any[] | IGroupRow;

export interface IClickSubsItem {
  handler(event: any, worldX: number, worldY: number, canvasX: number, canvasY: number): void;
  x: number;
  y: number;
  w: number;
  h: number;
}
