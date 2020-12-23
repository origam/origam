import { ICellRectangle } from "model/entities/TablePanelView/types/ICellRectangle";
import {IAggregation} from "../../../../../model/entities/types/IAggregation";

export interface IGroupRow {
  groupLevel: number;
  columnLabel: string;
  columnValue: string;
  isExpanded: boolean;
  sourceGroup: IGroupTreeNode;
  parent: IGroupRow | undefined;
}

export interface IGroupTreeNode {
  level: number;
  getRowIndex(rowId: string): number | undefined;
  parent: IGroupTreeNode | undefined;
  childGroups: IGroupTreeNode[];
  allChildGroups: IGroupTreeNode[];
  childRows: any[][];
  columnId: string;
  groupLabel: string;
  isExpanded: boolean;
  rowCount: number;
  columnValue: string;
  columnDisplayValue: string;
  aggregations: IAggregation[] | undefined;
  allParents: IGroupTreeNode[];
  composeGroupingFilter(): string;
  isInfinitelyScrolled: boolean;
  getRowById(id: string): any[] | undefined;
}

export type ITableRow = any[] | IGroupRow;

export interface IMouseOverSubsItem {
  toolTipGetter(event: any, worldX: number, worldY: number, canvasX: number, canvasY: number): IToolTipData;
  x: number;
  y: number;
  w: number;
  h: number;
}

export interface IToolTipData{
  content: any;
  columnIndex: number;
  rowIndex: number;
  cellWidth: number;
  cellHeight: number;
  positionRectangle: ICellRectangle;
}

export interface IClickSubsItem {
  handler(event: any, worldX: number, worldY: number, canvasX: number, canvasY: number): void;
  x: number;
  y: number;
  w: number;
  h: number;
}

export interface ICellOffset{
  row: number;
  column: number;
}