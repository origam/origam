
export interface IGroupRow {
  groupLevel: number;
  columnLabel: string;
  columnValue: string;
  isExpanded: boolean;
  sourceGroup: IGroupTreeNode;
  parent: IGroupRow | undefined;
}

export interface IGroupTreeNode {
  parent: IGroupTreeNode | undefined;
  childGroups: IGroupTreeNode[];
  childRows: any[][];
  columnId: string;
  groupLabel: string;
  isExpanded: boolean;
  rowCount: number;
  columnValue: string;
  columnDisplayValue: string;
}

export type ITableRow = any[] | IGroupRow;

export interface IClickSubsItem {
  handler(event: any, worldX: number, worldY: number, canvasX: number, canvasY: number): void;
  x: number;
  y: number;
  w: number;
  h: number;
}
