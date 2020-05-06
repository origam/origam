import {getGroupingConfiguration} from "../selectors/TablePanelView/getGroupingConfiguration";
import {getFormScreenLifecycle} from "model/selectors/FormScreen/getFormScreenLifecycle";
import {getDataView} from "model/selectors/DataView/getDataView";
import {IGrouper} from "./types/IGrouper";
import {observable} from "mobx";
import {IGroupTreeNode} from "gui/Components/ScreenElements/Table/TableRendering/types";
import {GroupItem} from "gui/Components/ScreenElements/Table/TableRendering/GroupItem";
import {getDataTable} from "../selectors/DataView/getDataTable";

export class ServerSideGrouper implements IGrouper {

  @observable.shallow topLevelGroups: IGroupTreeNode[] = []
  parent?: any = null;

  getTopLevelGroups(): IGroupTreeNode[] {
    return this.topLevelGroups
  }

  apply(firstGroupingColumn: string) {
    const dataView = getDataView(this);
    const property = getDataTable(this).getPropertyById(firstGroupingColumn);
    const lookupId = property && property.lookup && property.lookup.lookupId;
    getFormScreenLifecycle(this)
      .loadGroups(dataView, firstGroupingColumn, lookupId )
      .then(groupData =>this.topLevelGroups = this.group(groupData, firstGroupingColumn));
  }

  loadChildren(groupHeader: IGroupTreeNode) {
    const groupingConfiguration = getGroupingConfiguration(this);
    const nextColumnName = groupingConfiguration.nextColumnToGroupBy(groupHeader.columnId);
    const dataView = getDataView(this);
    const filter = this.composeGroupingFilter(groupHeader)
    const lifeCycle = getFormScreenLifecycle(this)
    if (nextColumnName) {
      lifeCycle
        .loadChildGroups(dataView, filter, nextColumnName)
        .then(groupData => groupHeader.childGroups = this.group(groupData, nextColumnName));
    }
    else {
      lifeCycle
        .loadChildRows(dataView, filter)
        .then(rows => groupHeader.childRows = rows)
    }
  }

  getAllParents(rowGroup: IGroupTreeNode) {
    let parent = rowGroup.parent
    const parents = []
    while (parent) {
      parents.push(parent);
      parent = rowGroup.parent
    }
    return parents;
  }

  toFilterValueForm(value: any){
    return typeof value === "string" ? "'" + value + "'" : value
  }

  composeGroupingFilter(rowGroup: IGroupTreeNode) {
    return this.getAllParents(rowGroup)
      .concat([rowGroup])
      .map(row => "[" + this.toFilterValueForm(row.columnValue) + ", eq, " + row.columnId + "]")
      .join(", ")
  }

  group(groupData: any[], columnId: string): IGroupTreeNode[] {
    const groupingConfiguration = getGroupingConfiguration(this);
    const level = groupingConfiguration.groupingIndices.get(columnId)

    if (!level) {
      throw new Error("Cannot find grouping index for column: " + columnId)
    }

    let dataTable = getDataTable(this);
    const property = dataTable.getPropertyById(columnId);

    return groupData
      .map(groupDataItem => {
        return new GroupItem({
          childGroups: [] as IGroupTreeNode[],
          childRows: [] as any[][],
          columnId: columnId,
          groupLabel: property!.name ,
          rowCount: groupDataItem["groupCount"] as number,
          parent: undefined,
          columnValue: groupDataItem[columnId],
          columnDisplayValue: groupDataItem["groupCaption"] || groupDataItem[columnId]}
      )});
  }
}