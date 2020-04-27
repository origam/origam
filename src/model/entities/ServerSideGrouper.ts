import { getGroupingConfiguration } from "../selectors/TablePanelView/getGroupingConfiguration";
import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";
import { getDataView } from "model/selectors/DataView/getDataView";
import { IGrouper } from "./types/IGrouper";
import { observable } from "mobx";
import { IGroupRow } from "gui/Components/ScreenElements/Table/TableRendering/types";

export class ServerSideGrouper implements IGrouper {

  @observable.shallow groupData: any[] = [];
  topLevelGroups: IGroupRow[] = []
  parent?: any = null;

  getTopLevelGroups(): IGroupRow[] {
    return this.topLevelGroups
  }

  apply(firstGroupingColumn: string) {
    const dataView = getDataView(this);
    getFormScreenLifecycle(this)
      .loadGroups(dataView, firstGroupingColumn)
      .then(groupData => this.groupData = groupData)
      .then(() => this.topLevelGroups = this.group(this.groupData, firstGroupingColumn));
  }

  loadChildren(groupHeader: IGroupRow) {
    const groupingConfiguration = getGroupingConfiguration(this);
    const nextColumnName = groupingConfiguration.nextColumnToGroupBy(groupHeader.columnLabel);
    const dataView = getDataView(this);
    const filter = this.composeGroupingFilter(groupHeader)
    const lifeCycle = getFormScreenLifecycle(this)
    if (nextColumnName) {
      lifeCycle
        .loadChildGroups(dataView, filter, nextColumnName)
        .then(groupData => groupHeader.sourceGroup.childRows = []); // = this.group(groupData, nextColumnName));
    }
    else {
      lifeCycle
        .loadChildRows(dataView, filter)
        .then(rows => groupHeader.sourceGroup.childRows = rows)
    }
  }

  getAllParents(rowGroup: IGroupRow) {
    let parent = rowGroup.parent
    const parents = []
    while (parent) {
      parents.push(parent);
      parent = rowGroup.parent
    }
    return parents;
  }

  composeGroupingFilter(rowGroup: IGroupRow) {
    const filterStrings = this.getAllParents(rowGroup)
      .concat([rowGroup])
      .map(row => "[" + row.columnValue + ", eq, " + row.sourceGroup.groupLabel + "]")
      .join(", ")

    return "[ AND, " + filterStrings + "]"
  }

  group(groupData: any[], columnName: string): IGroupRow[] {
    const groupingConfiguration = getGroupingConfiguration(this);
    const level = groupingConfiguration.groupingIndices.get(columnName)

    if (!level) {
      throw new Error("Cannot find grouping index for column: " + columnName)
    }

    return groupData
      .map(groupDataItem => {
        return {
          groupLevel: level,
          columnLabel: columnName,
          columnValue: groupDataItem[columnName],
          isExpanded: false,
          sourceGroup:{
            childGroups: [],
            childRows: [], 
            columnLabel: columnName,
            groupLabel:  groupDataItem["groupCaption"],
            rowCount: groupDataItem["groupCount"],
            isExpanded: false,
          },
          parent: undefined,
        };
      });
  }
}
