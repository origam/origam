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

import { getGroupingConfiguration } from "model/selectors/TablePanelView/getGroupingConfiguration";
import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";
import { getDataView } from "model/selectors/DataView/getDataView";
import { IGrouper } from "./types/IGrouper";
import { comparer, flow, IReactionDisposer, observable, reaction } from "mobx";
import { ICellOffset, IGroupTreeNode } from "gui/Components/ScreenElements/Table/TableRendering/types";
import { ServerSideGroupItem } from "gui/Components/ScreenElements/Table/TableRendering/GroupItem";
import { getDataTable } from "model/selectors/DataView/getDataTable";
import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";
import { getOrderingConfiguration } from "model/selectors/DataView/getOrderingConfiguration";
import { joinWithAND } from "./OrigamApiHelpers";
import { parseAggregations } from "./Aggregatioins";
import { getUserFilters } from "model/selectors/DataView/getUserFilters";
import { getFilterConfiguration } from "model/selectors/DataView/getFilterConfiguration";
import { getCellOffset, getNextRowId, getPreviousRowId, getRowById, getRowIndex } from "./GrouperCommon";
import _ from "lodash";
import { IGroupingSettings } from "./types/IGroupingConfiguration";
import { DateGroupData, GenericGroupData, IGroupData } from "./DateGroupData";
import moment from "moment";
import { runGeneratorInFlowWithHandler } from "utils/runInFlowWithHandler";


export class ServerSideGrouper implements IGrouper {
  @observable.shallow topLevelGroups: IGroupTreeNode[] = [];
  parent?: any = null;
  disposers: IReactionDisposer[] = [];
  groupDisposers: Map<IGroupTreeNode, IReactionDisposer> = new Map<IGroupTreeNode, IReactionDisposer>()
  @observable refreshTrigger = 0;

  start() {
    this.disposers.push(
      reaction(
        () => [
          Array.from(getGroupingConfiguration(this).groupingSettings.values()),
          Array.from(getGroupingConfiguration(this).groupingSettings.keys()),
          this.refreshTrigger],
        () => this.loadGroupsDebounced(),
        {fireImmediately: true, equals: comparer.structural, delay: 50})
    );
  }

  get allGroups() {
    return this.topLevelGroups.flatMap(group => [group, ...group.allChildGroups]);
  }

  loadGroupsDebounced = _.debounce(this.loadGroupsImm, 10);

  loadGroupsImm() {
    const self = this;
    runGeneratorInFlowWithHandler({ctx: this, generator: self.loadGroups()});
  }

  private*loadGroups(): any {
    const firstGroupingColumn = getGroupingConfiguration(this).firstGroupingColumn;
    if (!firstGroupingColumn) {
      this.topLevelGroups.length = 0;
      return;
    }
    const expandedGroupDisplayValues = this.allGroups
      .filter(group => group.isExpanded)
      .map(group => group.getColumnDisplayValue())
    const dataView = getDataView(this);
    const property = getDataTable(this).getPropertyById(firstGroupingColumn.columnId);
    const lookupId = property && property.lookup && property.lookup.lookupId;
    const aggregations = getTablePanelView(this).aggregations.aggregationList;
    const groupData = yield getFormScreenLifecycle(this).loadGroups(dataView, firstGroupingColumn, lookupId, aggregations)
    this.topLevelGroups = this.group(groupData, firstGroupingColumn.columnId, undefined);
    yield*this.loadAndExpandChildren(this.topLevelGroups, expandedGroupDisplayValues);
  }

  private*loadAndExpandChildren(childGroups: IGroupTreeNode[], expandedGroupDisplayValues: string[]): Generator {
    for (const group of childGroups) {
      if (expandedGroupDisplayValues.includes(group.getColumnDisplayValue())) {
        group.isExpanded = true;
        yield*this.loadChildren(group);
        yield*this.loadAndExpandChildren(group.childGroups, expandedGroupDisplayValues)
      }
    }
  }

  substituteRecords(rows: any[][]): void {
    this.allGroups.map(group => group.substituteRecords(rows))
  }

  refresh() {
    this.refreshTrigger++;
  }

  getRowIndex(rowId: string): number | undefined {
    return getRowIndex(this, rowId);
  }

  getRowById(id: string): any[] | undefined {
    return getRowById(this, id);
  }

  getTotalRowCount(rowId: string): number | undefined {
    return this.allGroups
      .find(group => group.getRowById(rowId))?.rowCount;
  }

  getCellOffset(rowId: string): ICellOffset {
    return getCellOffset(this, rowId);
  }

  getNextRowId(rowId: string): string {
    return getNextRowId(this, rowId);
  }

  getPreviousRowId(rowId: string): string {
    return getPreviousRowId(this, rowId);
  }

  notifyGroupClosed(group: IGroupTreeNode) {
    if (this.groupDisposers.has(group)) {
      this.groupDisposers.get(group)!();
      this.groupDisposers.delete(group);
    }
  }

  *loadChildren(groupHeader: IGroupTreeNode) {
    if (this.groupDisposers.has(groupHeader)) {
      this.groupDisposers.get(groupHeader)!();
    }
    this.groupDisposers.set(
      groupHeader,
      reaction(
        () => [
          getGroupingConfiguration(this).nextColumnToGroupBy(groupHeader.columnId),
          this.composeFinalFilter(groupHeader),
          [...getFilterConfiguration(this).activeFilters],
          [...getTablePanelView(this).aggregations.aggregationList],
          getOrderingConfiguration(this).groupChildrenOrdering
        ],
        () => this.loadChildrenReactionDebounced(groupHeader),
      )
    );
    yield*this.reload(groupHeader);
  }

  loadChildrenReactionDebounced = _.debounce(this.loadChildrenReaction, 10);

  private loadChildrenReaction(group: IGroupTreeNode) {
    flow(() => this.reload(group))();
  }

  private*reload(group: IGroupTreeNode): any {
    const groupingConfiguration = getGroupingConfiguration(this);
    const nextColumnSettings = groupingConfiguration.nextColumnToGroupBy(group.columnId);
    const dataView = getDataView(this);
    const filter = this.composeFinalFilter(group);
    const lifeCycle = getFormScreenLifecycle(this);
    const aggregations = getTablePanelView(this).aggregations.aggregationList;
    const orderingConfiguration = getOrderingConfiguration(this);
    if (nextColumnSettings) {
      const property = getDataTable(this).getPropertyById(nextColumnSettings.columnId);
      const lookupId = property && property.lookup && property.lookup.lookupId;
      const groupData = yield lifeCycle.loadChildGroups(dataView, filter, nextColumnSettings, aggregations, lookupId)
      group.childGroups = this.group(groupData, nextColumnSettings.columnId, group);
    } else {
      const rows = yield lifeCycle.loadChildRows(dataView, filter, orderingConfiguration.groupChildrenOrdering)
      group.childRows = rows;
    }
  }

  composeFinalFilter(rowGroup: IGroupTreeNode) {
    const groupingFilter = rowGroup.composeGroupingFilter();
    const userFilters = getUserFilters({ctx: this, excludePropertyId: rowGroup.columnId});

    return userFilters
      ? joinWithAND([groupingFilter, userFilters])
      : groupingFilter;
  }


  group(groupData: any[], columnId: string, parent: IGroupTreeNode | undefined): IGroupTreeNode[] {
    const groupingConfiguration = getGroupingConfiguration(this);
    const groupingSettings = groupingConfiguration.groupingSettings.get(columnId);
    const level = groupingSettings?.groupIndex;

    if (!level || !groupingSettings) {
      throw new Error("Cannot find grouping index for column: " + columnId);
    }

    let dataTable = getDataTable(this);
    const property = dataTable.getPropertyById(columnId);

    return groupData.map((groupDataItem) => {
      const groupData = this.getGroupData(groupDataItem, groupingSettings);
      return new ServerSideGroupItem({
        childGroups: [] as IGroupTreeNode[],
        childRows: [] as any[][],
        columnId: columnId,
        groupLabel: property!.name,
        rowCount: groupDataItem["groupCount"] as number,
        parent: parent,
        columnValue: groupData.value,
        getColumnDisplayValue: () => groupData.label,
        aggregations: parseAggregations(groupDataItem["aggregations"]),
        groupingUnit: groupingSettings.groupingUnit,
        grouper: this,
      });
    });
  }

  getGroupData(groupDataItem: any, groupingSettings: IGroupingSettings): IGroupData {
    if (!groupDataItem) {
      new DateGroupData(undefined, "");
    }
    if (groupingSettings.groupingUnit !== undefined) {

      const yearValue = groupDataItem[groupingSettings.columnId + "_year"];
      const monthValue = groupDataItem[groupingSettings.columnId + "_month"]
        ? groupDataItem[groupingSettings.columnId + "_month"] - 1
        : 0;
      const dayValue = groupDataItem[groupingSettings.columnId + "_day"] ?? 1
      const hourValue = groupDataItem[groupingSettings.columnId + "_hour"] ?? 0;
      const minuteValue = groupDataItem[groupingSettings.columnId + "_minute"] ?? 0;

      const value = moment({y: yearValue, M: monthValue, d: dayValue, h: hourValue, m: minuteValue, s: 0})
      return DateGroupData.create(value, groupingSettings.groupingUnit)
    } else {
      return new GenericGroupData(
        groupDataItem[groupingSettings.columnId],
        groupDataItem["groupCaption"] ?? groupDataItem[groupingSettings.columnId]
      );
    }
  }

  dispose() {
    for (let disposer of this.disposers) {
      disposer();
    }
    this.allGroups.forEach(group => group.dispose())
  }
}
