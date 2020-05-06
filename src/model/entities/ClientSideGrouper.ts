
import { IGrouper } from "./types/IGrouper";
import { getDataTable } from "model/selectors/DataView/getDataTable";
import { getGroupingConfiguration } from "model/selectors/TablePanelView/getGroupingConfiguration";
import { IGroupTreeNode } from "gui/Components/ScreenElements/Table/TableRendering/types";
import { GroupItem } from "gui/Components/ScreenElements/Table/TableRendering/GroupItem";

export class ClientSideGrouper implements IGrouper {

  topLevelGroups: IGroupTreeNode[] = []
  parent?: any = null;

  getTopLevelGroups(): IGroupTreeNode[] {
    return this.topLevelGroups;
  }

  apply(firstGroupingColumn: string): void {
    const dataTable = getDataTable(this);
    this.topLevelGroups = this.makeGroups(dataTable.allRows, firstGroupingColumn)
    this.loadRecursively(this.topLevelGroups);
  }

  loadRecursively(groups: IGroupTreeNode[]){
    for(let group of groups){
      if(group.isExpanded){
        this.loadChildren(group)
      }
      this.loadRecursively(group.childGroups);
    }
  }

  makeGroups(rows: any[][], groupingColumn: string): IGroupTreeNode[] {
    const index = this.findDataIndex(groupingColumn)
    return rows
      .map(row => row[index])
      .filter((v, i, a) => a.indexOf(v) === i) // distinct
      .map(groupName => {
        const groupRows = rows.filter(row => row[index] === groupName);
        return new GroupItem({
          childGroups: [] as IGroupTreeNode[],
          childRows: groupRows,
          columnId: groupingColumn,
          groupLabel:  groupName,
          rowCount: groupRows.length,
          parent: undefined,
          columnValue: groupName
        }
        );
      });
  }

  findGroupLevel(groupingColumn: string) {
    const groupingConfiguration = getGroupingConfiguration(this);
    const level = groupingConfiguration.groupingIndices.get(groupingColumn)
    if (!level) {
      return 0;
      throw new Error("Cannot find grouping index for column: " + groupingColumn)
    }
    return level;
  }

  findDataIndex(columnName: string) {
    const dataTable = getDataTable(this);
    const property = dataTable.getPropertyById(columnName)
    if (!property) {
      return 0;
      throw new Error("Cannot find property named: " + columnName)
    }
    return property.dataIndex
  }

  loadChildren(groupHeader: IGroupTreeNode): void {
    const groupingConfiguration = getGroupingConfiguration(this);
    const nextColumnName = groupingConfiguration.nextColumnToGroupBy(groupHeader.columnId);

    if (nextColumnName) {
      groupHeader.childGroups = this.makeGroups(groupHeader.childRows, nextColumnName)
    }
  }
}
