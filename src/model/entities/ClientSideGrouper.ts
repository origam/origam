import {IGrouper} from "./types/IGrouper";
import {getDataTable} from "model/selectors/DataView/getDataTable";
import {getGroupingConfiguration} from "model/selectors/TablePanelView/getGroupingConfiguration";
import {IGroupTreeNode} from "gui/Components/ScreenElements/Table/TableRendering/types";
import {GroupItem} from "gui/Components/ScreenElements/Table/TableRendering/GroupItem";
import {getTablePanelView} from "../selectors/TablePanelView/getTablePanelView";
import {AggregationType, IAggregationInfo} from "./types/IAggregationInfo";

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

  loadRecursively(groups: IGroupTreeNode[]) {
    for (let group of groups) {
      if (group.isExpanded) {
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
          groupLabel: groupName,
          rowCount: groupRows.length,
          parent: undefined,
          columnValue: groupName,
          columnDisplayValue: groupName,
          aggregations: this.calcAggregations(groupRows)
        });
      });
  }

  calcAggregations(rows: any[][]) {
    return getTablePanelView(this)
      .aggregations.get()
      .map(aggregationInfo => {
        return {
          columnId: aggregationInfo.ColumnName,
          type: aggregationInfo.AggregationType,
          value: this.calcAggregation(aggregationInfo, rows)
        }
      });
  }


  private calcAggregation(aggregationInfo: IAggregationInfo, rows: any[][]) {
    const index = this.findDataIndex(aggregationInfo.ColumnName);
    const valuesToAggregate = rows.map(row => row[index]);

    switch (aggregationInfo.AggregationType) {
      case AggregationType.SUM:
        return valuesToAggregate.reduce((a, b) => a + b, 0);
      case AggregationType.AVG:
        return (valuesToAggregate.reduce((a, b) => a + b, 0)) / rows.length;
      case AggregationType.MIN:
        return Math.min(...valuesToAggregate);
      case AggregationType.MAX:
        return Math.max(...valuesToAggregate);
      default:
        throw new Error("Aggregation type not implemented: " + aggregationInfo.AggregationType)
    }
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

class AggregationCalculator {

}
