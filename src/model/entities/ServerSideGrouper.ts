import { getGroupingConfiguration } from "model/selectors/TablePanelView/getGroupingConfiguration";
import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";
import { getDataView } from "model/selectors/DataView/getDataView";
import { IGrouper } from "./types/IGrouper";
import { autorun, IReactionDisposer, observable } from "mobx";
import { IGroupTreeNode } from "gui/Components/ScreenElements/Table/TableRendering/types";
import { ServerSideGroupItem } from "gui/Components/ScreenElements/Table/TableRendering/GroupItem";
import { getDataTable } from "model/selectors/DataView/getDataTable";
import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";
import { getOrderingConfiguration } from "model/selectors/DataView/getOrderingConfiguration";
import { parseAggregations } from "./types/IAggregation";
import { joinWithAND, toFilterItem } from "./OrigamApiHelpers";

export class ServerSideGrouper implements IGrouper {
  @observable.shallow topLevelGroups: IGroupTreeNode[] = [];
  parent?: any = null;
  disposers: IReactionDisposer[] = [];

  start() {
    this.disposers.push(
      autorun(() => {
        const firstGroupingColumn = getGroupingConfiguration(this).firstGroupingColumn;
        if (!firstGroupingColumn) {
          this.topLevelGroups.length = 0;
          return;
        }
        const dataView = getDataView(this);
        const property = getDataTable(this).getPropertyById(firstGroupingColumn);
        const lookupId = property && property.lookup && property.lookup.lookupId;
        const aggregations = getTablePanelView(this).aggregations.aggregationList;
        getFormScreenLifecycle(this)
          .loadGroups(dataView, firstGroupingColumn, lookupId, aggregations)
          .then(
            (groupData) =>
              (this.topLevelGroups = this.group(groupData, firstGroupingColumn, undefined))
          );
      })
    );
  }

  loadChildren(groupHeader: IGroupTreeNode) {
    this.disposers.push(
      autorun(() => {
        const groupingConfiguration = getGroupingConfiguration(this);
        const nextColumnName = groupingConfiguration.nextColumnToGroupBy(groupHeader.columnId);
        const dataView = getDataView(this);
        const filter = this.composeGroupingFilter(groupHeader);
        const lifeCycle = getFormScreenLifecycle(this);
        const aggregations = getTablePanelView(this).aggregations.aggregationList;
        const orderingConfiguration = getOrderingConfiguration(this);
        if (nextColumnName) {
          const property = getDataTable(this).getPropertyById(nextColumnName);
          const lookupId = property && property.lookup && property.lookup.lookupId;
          lifeCycle
            .loadChildGroups(dataView, filter, nextColumnName, aggregations, lookupId)
            .then(
              (groupData) =>
                (groupHeader.childGroups = this.group(groupData, nextColumnName, groupHeader))
            );
        } else {
          lifeCycle
            .loadChildRows(dataView, filter, orderingConfiguration.groupChildrenOrdering)
            .then((rows) => (groupHeader.childRows = rows));
        }
      })
    );
  }

  getAllParents(rowGroup: IGroupTreeNode) {
    let parent = rowGroup.parent;
    const parents = [];
    while (parent) {
      parents.push(parent);
      parent = parent.parent;
    }
    return parents;
  }

  composeGroupingFilter(rowGroup: IGroupTreeNode) {
    const parents = this.getAllParents(rowGroup);
    if (parents.length === 0) {
      return toFilterItem(rowGroup.columnId, null, "eq", rowGroup.columnValue);
    } else {
      const andOperands = parents
        .concat([rowGroup])
        .map((row) => toFilterItem(row.columnId, null, "eq", row.columnValue));
      return joinWithAND(andOperands);
    }
  }

  group(groupData: any[], columnId: string, parent: IGroupTreeNode | undefined): IGroupTreeNode[] {
    const groupingConfiguration = getGroupingConfiguration(this);
    const level = groupingConfiguration.groupingIndices.get(columnId);

    if (!level) {
      throw new Error("Cannot find grouping index for column: " + columnId);
    }

    let dataTable = getDataTable(this);
    const property = dataTable.getPropertyById(columnId);

    return groupData.map((groupDataItem) => {
      return new ServerSideGroupItem({
        childGroups: [] as IGroupTreeNode[],
        childRows: [] as any[][],
        columnId: columnId,
        groupLabel: property!.name,
        rowCount: groupDataItem["groupCount"] as number,
        parent: parent,
        columnValue: groupDataItem[columnId],
        columnDisplayValue: groupDataItem["groupCaption"] || groupDataItem[columnId],
        aggregations: parseAggregations(groupDataItem["aggregations"]),
        grouper: this,
      });
    });
  }

  dispose() {
    for (let disposer of this.disposers) {
      disposer();
    }
  }
}
