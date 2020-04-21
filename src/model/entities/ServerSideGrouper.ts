import { IRowGroup } from "./types/IRowGroup";
import { getGroupingConfiguration } from "../selectors/TablePanelView/getGroupingConfiguration";
import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";
import { getDataView } from "model/selectors/DataView/getDataView";
import { IGrouper } from "./types/IGrouper";
import { observable } from "mobx";

export class ServerSideGrouper implements IGrouper {

  @observable.shallow groupData: any[] = [];
  topLevelGroups: IRowGroup[] = []

  getTopLevelGroups(): IRowGroup[] {
    return this.topLevelGroups
  }

  apply(firstGroupingColumn: string) {
    const dataView = getDataView(this);
    getFormScreenLifecycle(this)
      .loadGroups(dataView, firstGroupingColumn)
      .then(groupData => this.groupData = groupData)
      .then(() => this.topLevelGroups = this.group(this.groupData, firstGroupingColumn));
  }

  loadChildren(groupHeader: IRowGroup) {
    const groupingConfiguration = getGroupingConfiguration(this);
    const nextColumnName = groupingConfiguration.nextColumnToGroupBy(groupHeader.groupColumnName);
    const dataView = getDataView(this);
    const filter = this.composeGroupingFilter(groupHeader)
    const lifeCycle = getFormScreenLifecycle(this)
    if (nextColumnName) {
      lifeCycle
        .loadChildGroups(dataView, filter, nextColumnName)
        .then(groupData => groupHeader.groupChildren = this.group(groupData, nextColumnName));
    }
    else {
      lifeCycle
        .loadChildRows(dataView, filter)
        .then(rows => groupHeader.rowChildren = rows)
    }
  }

  getAllParents(rowGroup: IRowGroup) {
    let parent = rowGroup.parent
    const parents = []
    while (parent) {
      parents.push(parent);
      parent = rowGroup.parent
    }
    return parents;
  }

  composeGroupingFilter(rowGroup: IRowGroup) {
    const filterStrings = this.getAllParents(rowGroup)
      .concat([rowGroup])
      .map(row => "[" + row.groupColumnName + ", eq, " + row.groupValue + "]")
      .join(", ")

    return "[ AND, " + filterStrings + "]"
  }

  group(groupData: any[], columnName: string): IRowGroup[] {
    const groupingConfiguration = getGroupingConfiguration(this);
    const level = groupingConfiguration.groupingIndices.get(columnName)

    if (!level) {
      throw new Error("Cannot find grouping index for column: " + columnName)
    }

    return groupData
      .map(groupDataItem => {
        return {
          isExpanded: false,
          level: level,
          groupColumnName: columnName,
          groupValue: groupDataItem[columnName],
          groupCaption: groupDataItem["groupCaption"],
          rowCount: groupDataItem["groupCount"],
          groupChildren: [],
          rowChildren: [],
          parent: undefined
        };
      });
  }
}
