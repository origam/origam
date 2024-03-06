/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

import { ICellRectangle } from "model/entities/TablePanelView/types/ICellRectangle";
import { IAggregation } from "../../../../../model/entities/types/IAggregation";

export interface IGroupRow {
  groupLevel: number;
  columnLabel: string;
  columnValue: string;
  isExpanded: boolean;
  sourceGroup: IGroupTreeNode;
  parent: IGroupRow | undefined;
}

export interface IGroupTreeNode {
  dispose(): void;

  substituteRecords(rows: any[][]): any;

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
  getColumnDisplayValue: () => string;
  aggregations: IAggregation[] | undefined;
  allParents: IGroupTreeNode[];

  composeGroupingFilter(): string;

  isInfinitelyScrolled: boolean;

  getRowById(id: string): any[] | undefined;

  groupFilters: string[];
}

export type ITableRow = any[] | IGroupRow;

export interface IMouseOverSubsItem {
  tooltipGetter(worldX: number, worldY: number, canvasX: number, canvasY: number): ITooltipData;

  x: number;
  y: number;
  w: number;
  h: number;
}

export interface ITooltipData {
  content: any;
  columnIndex: number;
  rowIndex: number;
  cellWidth: number;
  cellHeight: number;
  positionRectangle: ICellRectangle;
}

export interface IClickSubsItem {
  handler(event: any, worldX: number, worldY: number, canvasX: number, canvasY: number): Promise<void>;

  x: number;
  y: number;
  w: number;
  h: number;
}

export interface IMouseMoveSubsItem {
  handler(event: any, worldX: number, worldY: number, canvasX: number, canvasY: number): void;

  x: number;
  y: number;
  w: number;
  h: number;
}

export interface ICellOffset {
  row: number;
  column: number;
}