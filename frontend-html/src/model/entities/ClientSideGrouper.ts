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

import { IGrouper } from "./types/IGrouper";
import { getDataTable } from "model/selectors/DataView/getDataTable";
import { getGroupingConfiguration } from "model/selectors/TablePanelView/getGroupingConfiguration";
import { ICellOffset, IGroupTreeNode } from "gui/Components/ScreenElements/Table/TableRendering/types";
import { ClientSideGroupItem } from "gui/Components/ScreenElements/Table/TableRendering/GroupItem";
import { getTablePanelView } from "../selectors/TablePanelView/getTablePanelView";
import { IAggregationInfo } from "./types/IAggregationInfo";
import { computed } from "mobx";
import { AggregationType } from "./types/AggregationType";
import { getCellOffset, getNextRowId, getPreviousRowId, getRowById, getRowCount, getRowIndex } from "./GrouperCommon";
import { IGroupingSettings } from "./types/IGroupingConfiguration";
import { DateGroupData, GenericGroupData, IGroupData } from "./DateGroupData";
import moment from "moment";
import { getOrderingConfiguration } from "model/selectors/DataView/getOrderingConfiguration";
import { IOrderByDirection } from "./types/IOrderingConfiguration";

export class ClientSideGrouper implements IGrouper {
  parent?: any = null;
  expandedGroupDisplayValues: Set<string> = new Set();

  @computed
  get topLevelGroups() {
    const firstGroupingColumn = getGroupingConfiguration(this).firstGroupingColumn;
    if (firstGroupingColumn === undefined) {
      return [];
    }
    const dataTable = getDataTable(this);
    const groups = this.makeGroups(undefined, dataTable.rows, firstGroupingColumn);
    this.loadRecursively(groups);
    return groups;
  }

  get allGroups() {
    return this.topLevelGroups.flatMap(group => [group, ...group.allChildGroups]);
  }
  substituteRecords(rows: any[][]) {
  }

  substituteRecord(row: any[]): void {
  }

  getCellOffset(rowId: string): ICellOffset {
    return getCellOffset(this, rowId);
  }

  getRowIndex(rowId: string): number | undefined {
    return getRowIndex(this, rowId);
  }

  getRowById(id: string): any[] | undefined {
    return getRowById(this, id);
  }

  getTotalRowCount(rowId: string): number | undefined {
    return getRowCount(this, rowId);
  }

  getNextRowId(rowId: string): string {
    return getNextRowId(this, rowId);
  }

  getPreviousRowId(rowId: string): string {
    return getPreviousRowId(this, rowId);
  }

  loadRecursively(groups: IGroupTreeNode[]) {
    for (let group of groups) {
      if (this.expandedGroupDisplayValues.has(group.getColumnDisplayValue())) {
        group.isExpanded = true;
        this.loadChildrenInternal(group);
        this.loadRecursively(group.childGroups);
      }
    }
  }

  expansionListener(item: ClientSideGroupItem) {
    if (item.isExpanded) {
      this.expandedGroupDisplayValues.add(item.getColumnDisplayValue());
    } else {
      this.expandedGroupDisplayValues.delete(item.getColumnDisplayValue());
    }
  }

  makeGroups(parent: IGroupTreeNode | undefined, rows: any[][], groupingColumnSettings: IGroupingSettings): IGroupTreeNode[] {
    const dataTable = getDataTable(this);
    const property = dataTable.getPropertyById(groupingColumnSettings.columnId);
    const orderingConfig = getOrderingConfiguration(this);
    const orderingDirection = orderingConfig.orderings
        .find(ordering => ordering.columnId === groupingColumnSettings.columnId)
        ?.direction
      ?? IOrderByDirection.ASC;

    return this.groupToGroupDataList(groupingColumnSettings, rows)
      .sort((a, b) =>
        orderingDirection === IOrderByDirection.ASC
          ? a.compare(b)
          : -a.compare(b))
      .map((groupData) => {
        return new ClientSideGroupItem({
          childGroups: [] as IGroupTreeNode[],
          childRows: groupData.rows,
          columnId: groupingColumnSettings.columnId,
          groupLabel: property!.name,
          rowCount: groupData.rows.length,
          parent: parent,
          columnValue: groupData.label,
          getColumnDisplayValue: () => property ? dataTable.resolveCellText(property, groupData.label) : groupData.label,
          aggregations: this.calcAggregations(groupData.rows),
          grouper: this,
          expansionListener: this.expansionListener.bind(this)
        });
      });
  }

  private groupToGroupDataList(groupingSettings: IGroupingSettings | undefined, rows: any[][]) {
    if (!groupingSettings) {
      return [];
    }

    const index = this.findDataIndex(groupingSettings.columnId);
    const groupMap = new Map<string, IGroupData>();
    for (let row of rows) {
      const groupData = groupingSettings.groupingUnit === undefined
        ? new GenericGroupData(row[index], row[index])
        : DateGroupData.create(moment(row[index]), groupingSettings.groupingUnit)
      if (!groupMap.has(groupData.label)) {
        groupMap.set(groupData.label, groupData);
      }
      groupMap.get(groupData.label)!.rows!.push(row);
    }

    return Array.from(groupMap.values());
  }

  calcAggregations(rows: any[][]) {
    return getTablePanelView(this).aggregations.aggregationList.map((aggregationInfo) => {
      return {
        columnId: aggregationInfo.ColumnName,
        type: aggregationInfo.AggregationType,
        value: this.calcAggregation(aggregationInfo, rows),
      };
    });
  }

  private calcAggregation(aggregationInfo: IAggregationInfo, rows: any[][]) {
    const index = this.findDataIndex(aggregationInfo.ColumnName);
    const valuesToAggregate = rows.map((row) => row[index]);

    switch (aggregationInfo.AggregationType) {
      case AggregationType.SUM:
        return valuesToAggregate.reduce((a, b) => a + b, 0);
      case AggregationType.AVG:
        return valuesToAggregate.reduce((a, b) => a + b, 0) / rows.length;
      case AggregationType.MIN:
        return Math.min(...valuesToAggregate);
      case AggregationType.MAX:
        return Math.max(...valuesToAggregate);
      default:
        throw new Error("Aggregation type not implemented: " + aggregationInfo.AggregationType);
    }
  }

  findDataIndex(columnName: string) {
    const dataTable = getDataTable(this);
    const property = dataTable.getPropertyById(columnName);
    if (!property) {
      return 0;
    }
    return property.dataIndex;
  }

  *loadChildren(group: IGroupTreeNode) {
    this.loadChildrenInternal(group);
  }

  loadChildrenInternal(group: IGroupTreeNode) {
    const groupingConfiguration = getGroupingConfiguration(this);
    const nextColumnName = groupingConfiguration.nextColumnToGroupBy(group.columnId);

    if (nextColumnName) {
      group.childGroups = this.makeGroups(group, group.childRows, nextColumnName);
    }
  }


  notifyGroupClosed(group: IGroupTreeNode) {
  }

  start(): void {
  }
}



