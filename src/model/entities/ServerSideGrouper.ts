import {getGroupingConfiguration} from "../selectors/TablePanelView/getGroupingConfiguration";
import {getFormScreenLifecycle} from "model/selectors/FormScreen/getFormScreenLifecycle";
import {getDataView} from "model/selectors/DataView/getDataView";
import {IGrouper} from "./types/IGrouper";
import {observable} from "mobx";
import {IGroupTreeNode} from "gui/Components/ScreenElements/Table/TableRendering/types";
import {GroupItem, parseAggregations} from "gui/Components/ScreenElements/Table/TableRendering/GroupItem";
import {getDataTable} from "../selectors/DataView/getDataTable";
import {getTablePanelView} from "../selectors/TablePanelView/getTablePanelView";
import {IDataTable} from "./types/IDataTable";

export class ServerSideGrouper implements IGrouper {

  @observable.shallow topLevelGroups: IGroupTreeNode[] = []
  parent?: any = null;
  @observable sortingFunction: ((dataTable: IDataTable) => (row1: any[], row2: any[]) => number) | undefined = undefined;

  getTopLevelGroups(): IGroupTreeNode[] {
    return this.topLevelGroups
  }

  apply(firstGroupingColumn: string) {
    const dataView = getDataView(this);
    const property = getDataTable(this).getPropertyById(firstGroupingColumn);
    const lookupId = property && property.lookup && property.lookup.lookupId;
    const aggregations = getTablePanelView(this).aggregations.get();
    getFormScreenLifecycle(this)
      .loadGroups(dataView, firstGroupingColumn, lookupId, aggregations)
      .then(groupData =>this.topLevelGroups = this.group(groupData, firstGroupingColumn, undefined));
  }

  loadChildren(groupHeader: IGroupTreeNode) {
    const groupingConfiguration = getGroupingConfiguration(this);
    const nextColumnName = groupingConfiguration.nextColumnToGroupBy(groupHeader.columnId);
    const dataView = getDataView(this);
    const filter = this.composeGroupingFilter(groupHeader)
    const lifeCycle = getFormScreenLifecycle(this)
    const aggregations = getTablePanelView(this).aggregations.get();
    if (nextColumnName) {
      lifeCycle
        .loadChildGroups(dataView, filter, nextColumnName, aggregations)
        .then(groupData => groupHeader.childGroups = this.group(groupData, nextColumnName, groupHeader));
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
      parent = parent.parent
    }
    return parents;
  }

  toFilterValueForm(value: any){
    return typeof value === "string" ? "\"" + value + "\"" : value
  }

  composeGroupingFilter(rowGroup: IGroupTreeNode) {
    const parents = this.getAllParents(rowGroup);
    if(parents.length === 0){
      return this.rowToFilterItem(rowGroup);
    }else{
      const andOperands =  this.getAllParents(rowGroup)
          .concat([rowGroup])
          .map(row => this.rowToFilterItem(row))
          .join(", ")
      return "[\"$AND\", " + andOperands + "]"
    }
  }

  rowToFilterItem(row: IGroupTreeNode){
    return "[\"" + row.columnId  + "\", \"eq\", " + this.toFilterValueForm(row.columnValue)+ "]";
  }

  group(groupData: any[], columnId: string, parent: IGroupTreeNode | undefined): IGroupTreeNode[] {
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
              groupLabel: property!.name,
              rowCount: groupDataItem["groupCount"] as number,
              parent: parent,
              columnValue: groupDataItem[columnId],
              columnDisplayValue: groupDataItem["groupCaption"] || groupDataItem[columnId],
              aggregations: parseAggregations(groupDataItem["aggregations"]),
              grouper: this
            }
        )});
  }
}