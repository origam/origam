import { IRowGroup } from "./types/IRowGroup";
import { IGrouper } from "./types/IGrouper";
import { getDataTable } from "model/selectors/DataView/getDataTable";
import { getGroupingConfiguration } from "model/selectors/TablePanelView/getGroupingConfiguration";

export class ClientSideGrouper implements IGrouper {

  topLevelGroups: IRowGroup[] = []

  getTopLevelGroups(): IRowGroup[] {
    return this.topLevelGroups;
  }

  apply(firstGroupingColumn: string): void {
    const dataTable = getDataTable(this);
    this.topLevelGroups = this.makeGroups(dataTable.allRows, firstGroupingColumn)
  }

  makeGroups(rows: any[][], groupingColumn: string): IRowGroup[] {
    const level = this.findGroupLevel(groupingColumn)
    const index = this.findDataIndex(groupingColumn)
    return rows
      .map(row => row[index])
      .filter((v, i, a) => a.indexOf(v) === i) // distinct
      .map(groupName => {
        const groupRows = rows.filter(row => row[index] === groupName);
        return {
          isExpanded: false,
          level: level,
          groupColumnName: groupingColumn,
          groupValue: groupName,
          groupCaption: groupName,
          rowCount: groupRows.length,
          groupChildren: [],
          rowChildren: groupRows,
          parent: undefined
        };
      });
  }

  findGroupLevel(groupingColumn: string) {
    const groupingConfiguration = getGroupingConfiguration(this);
    const level = groupingConfiguration.groupingIndices.get(groupingColumn)
    if (!level) {
      throw new Error("Cannot find grouping index for column: " + groupingColumn)
    }
    return level;
  }

  findDataIndex(columnName: string) {
    const dataTable = getDataTable(this);
    const property = dataTable.getPropertyById(columnName)
    if (!property) {
      throw new Error("Cannot find property named: " + columnName)
    }
    return property.dataIndex
  }

  loadChildren(groupHeader: IRowGroup): void {
    const groupingConfiguration = getGroupingConfiguration(this);
    const nextColumnName = groupingConfiguration.nextColumnToGroupBy(groupHeader.groupColumnName);

    if (nextColumnName) {
      groupHeader.groupChildren = this.makeGroups(groupHeader.rowChildren, nextColumnName)
    }
  }
}
