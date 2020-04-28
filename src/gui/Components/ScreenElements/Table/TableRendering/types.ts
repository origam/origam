
export interface IGroupRow {
  groupLevel: number;
  columnLabel: string;
  columnValue: string;
  isExpanded: boolean;
  sourceGroup: IGroupTreeNode;
  parent: IGroupRow | undefined;
}

export interface IGroupTreeNode {
  childGroups: IGroupTreeNode[];
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
