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

import { computed, observable } from "mobx";
import { IGroupTreeNode } from "./types";
import { IGrouper } from "../../../../../model/entities/types/IGrouper";
import { IAggregation } from "../../../../../model/entities/types/IAggregation";
import { getOrderingConfiguration } from "../../../../../model/selectors/DataView/getOrderingConfiguration";
import {
  InfiniteScrollLoader,
  SCROLL_ROW_CHUNK
} from "../../../../Workbench/ScreenArea/TableView/InfiniteScrollLoader";
import { getDataView } from "../../../../../model/selectors/DataView/getDataView";
import { joinWithAND, toFilterItem } from "../../../../../model/entities/OrigamApiHelpers";
import { OpenGroupVisibleRowsMonitor } from "../../../../Workbench/ScreenArea/TableView/VisibleRowsMonitor";
import { getDataTable } from "../../../../../model/selectors/DataView/getDataTable";
import { ScrollRowContainer } from "../../../../../model/entities/ScrollRowContainer";
import { GroupingUnit } from "model/entities/types/GroupingUnit";
import moment from "moment";

export interface IGroupItemData {
  childGroups: IGroupTreeNode[];
  childRows: any[][];
  columnId: string;
  columnValue: string;
  getColumnDisplayValue: () => string;
  groupLabel: string;
  parent: IGroupTreeNode | undefined;
  rowCount: number;
  aggregations: IAggregation[] | undefined;
  grouper: IGrouper;
}

export interface IServerSideGroupItemData extends IGroupItemData {
  groupingUnit: GroupingUnit | undefined;
}

export interface IClientSideGroupItemData extends IGroupItemData {
  expansionListener: (item: ClientSideGroupItem) => void;
}

export class ClientSideGroupItem implements IClientSideGroupItemData, IGroupTreeNode {
  constructor(data: IClientSideGroupItemData) {
    Object.assign(this, data);
  }

  isInfinitelyScrolled = false;
  expansionListener: (item: ClientSideGroupItem) => void = null as any;
  @observable childGroups: IGroupTreeNode[] = null as any;
  @observable _childRows: any[][] = null as any;
  columnId: string = null as any;
  columnValue: string = null as any;
  groupLabel: string = null as any;
  parent: IGroupTreeNode | undefined = null as any;
  rowCount: number = null as any;
  getColumnDisplayValue: () => string = null as any;
  aggregations: IAggregation[] | undefined = undefined;
  grouper: IGrouper = null as any;
  groupFilters: string[] = [];

  get level() {
    return this.allParents.length;
  }

  @observable
  private _isExpanded = false;

  public get isExpanded() {
    return this._isExpanded;
  }

  public set isExpanded(value) {
    this._isExpanded = value;
    this.expansionListener(this);
  }

  get allChildGroups(): IGroupTreeNode[] {
    return allChildGroups(this);
  }

  get allParents(): IGroupTreeNode[] {
    return getAllParents(this);
  }

  substituteRecords(rows: any[][]): void {
  }

  getRowIndex(rowId: string): number | undefined {
    return this._childRows.findIndex(row => getDataTable(this.grouper).getRowId(row) === rowId);
  }

  getRowById(id: string): any[] | undefined {
    return this._childRows.find(row => getDataTable(this.grouper).getRowId(row) === id);
  }

  @computed get childRows() {
    const orderingConfiguration = getOrderingConfiguration(this.grouper);

    if (orderingConfiguration.userOrderings.length === 0) {
      return this._childRows;
    } else {
      return this._childRows.slice().sort(orderingConfiguration.orderingFunction());
    }
  }

  set childRows(rows: any[][]) {
    this._childRows = rows;
  }

  composeGroupingFilter(): string {
    throw new Error("Method not implemented.");
  }

  dispose(): void {
  }
}

export class ServerSideGroupItem implements IGroupTreeNode {
  constructor(data: IServerSideGroupItemData) {
    const dataTable = getDataTable(data.grouper);
    this._childRows = new ScrollRowContainer(
      (row: any[]) => dataTable.getRowId(row),
      dataTable);
    Object.assign(this, data);

    const dataView = getDataView(this.grouper);
    this.scrollLoader = new InfiniteScrollLoader({
      ctx: this.grouper,
      gridDimensions: dataView.gridDimensions,
      scrollState: dataView.scrollState,
      rowsContainer: this._childRows,
      groupFilter: this.composeGroupingFilter(),
      visibleRowsMonitor: new OpenGroupVisibleRowsMonitor(this.grouper, dataView.gridDimensions, dataView.scrollState)
    })
    this.scrollLoader.registerAppendListener(data => dataTable.appendRecords(data))
    this.scrollLoader.registerPrependListener(data => dataTable.appendRecords(data))
  }

  @observable childGroups: IGroupTreeNode[] = null as any;
  columnId: string = null as any;
  columnValue: string = null as any;
  groupLabel: string = null as any;
  parent: IGroupTreeNode | undefined = null as any;
  rowCount: number = null as any;
  getColumnDisplayValue: () => string = null as any;
  aggregations: IAggregation[] | undefined = undefined;
  grouper: IGrouper = null as any;
  scrollLoader: InfiniteScrollLoader;
  _childRows: ScrollRowContainer;
  groupingUnit: GroupingUnit = null as any;


  get level() {
    return this.allParents.length;
  }

  get isInfinitelyScrolled() {
    return this.rowCount >= SCROLL_ROW_CHUNK && this.isExpanded && this.childRows.length > 0
  }

  get allChildGroups(): IGroupTreeNode[] {
    return allChildGroups(this);
  }

  get allParents(): IGroupTreeNode[] {
    return getAllParents(this);
  }

  substituteRecords(rows: any[][]): any {
    this._childRows.substituteRows(rows);
  }

  getRowIndex(rowId: string): number | undefined {
    return this.childRows.findIndex(row => getDataTable(this.grouper).getRowId(row) === rowId);
  }

  getRowById(id: string): any[] | undefined {
    return this.childRows.find(row => getDataTable(this.grouper).getRowId(row) === id);
  }

  @computed get childRows() {
    return this._childRows.rows;
  }

  set childRows(rows: any[][]) {
    if (rows.length > 0) {
      this.scrollLoader.start();
      getDataTable(this.grouper).appendRecords(rows);
    }
    this._childRows.set(rows);
  }

  get groupFilters() {
    if (this.groupingUnit !== undefined) {
      const momentValueStart = moment(this.columnValue);
      const momentValueEnd = moment(this.columnValue);
      switch (this.groupingUnit) {
        case GroupingUnit.Year:
          momentValueEnd.set({'year': momentValueStart.year() + 1});
          break;
        case GroupingUnit.Month:
          momentValueEnd.set({'month': momentValueStart.month() + 1});
          break;
        case GroupingUnit.Day:
          momentValueEnd.set({'day': momentValueStart.day() + 1});
          break;
        case GroupingUnit.Hour:
          momentValueEnd.set({'hour': momentValueStart.hour() + 1});
          break;
        case GroupingUnit.Minute:
          momentValueEnd.set({'minute': momentValueStart.minute() + 1});
          break;
        default:
          throw new Error("Filter generation for groupingUnit:" + this.groupingUnit + " not implemented");
      }
      return [
        toFilterItem(this.columnId, null, "gte", momentValueStart),
        toFilterItem(this.columnId, null, "lt", momentValueEnd)
      ];
    } else {
      return [toFilterItem(this.columnId, null, "eq", this.columnValue)]
    }
  }

  composeGroupingFilter(): string {
    const filters = getAllParents(this)
      .concat([this])
      .flatMap(groupNode => groupNode.groupFilters)
    return joinWithAND(filters);
  }

  @observable private _isExpanded = false;

  get isExpanded(): boolean {
    return this._isExpanded;
  }

  set isExpanded(value: boolean) {
    if (!value) {
      this.grouper.notifyGroupClosed(this);
    }
    this._isExpanded = value;
  }

  dispose(): void {
    this.scrollLoader.dispose();
  }
}

function getAllParents(group: IGroupTreeNode) {
  const parents: IGroupTreeNode[] = [];
  let parent = group.parent;
  while (parent) {
    parents.push(parent);
    parent = parent.parent;
  }
  return parents;
}

function allChildGroups(group: IGroupTreeNode): IGroupTreeNode[] {
  const allChildGroups = group.childGroups.flatMap(childGroup => childGroup.allChildGroups)
  return [...group.childGroups, ...allChildGroups];
}
