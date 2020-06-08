import {IGrouper} from "./types/IGrouper";
import {getDataTable} from "model/selectors/DataView/getDataTable";
import {getGroupingConfiguration} from "model/selectors/TablePanelView/getGroupingConfiguration";
import {IGroupTreeNode} from "gui/Components/ScreenElements/Table/TableRendering/types";
import {ClientSideGroupItem} from "gui/Components/ScreenElements/Table/TableRendering/GroupItem";
import {getTablePanelView} from "../selectors/TablePanelView/getTablePanelView";
import {IAggregationInfo} from "./types/IAggregationInfo";
import { computed } from "mobx";
import {AggregationType} from "./types/AggregationType";

export class ClientSideGrouper implements IGrouper {

  parent?: any = null;

  @computed get topLevelGroups(){
    const firstGroupingColumn = getGroupingConfiguration(this).firstGroupingColumn;
    const dataTable = getDataTable(this);
    const groups = this.makeGroups(dataTable.allRows, firstGroupingColumn)
    this.loadRecursively(groups);
    return groups;
  }

  loadRecursively(groups: IGroupTreeNode[]) {
    for (let group of groups) {
      this.loadChildren(group)
      this.loadRecursively(group.childGroups);
    }
  }

  makeGroups(rows: any[][], groupingColumn: string): IGroupTreeNode[] {
    const groupMap = this.makeGroupMap(groupingColumn, rows);

    let dataTable = getDataTable(this);
    const property = dataTable.getPropertyById(groupingColumn);

    return Array.from(groupMap.entries())
      .map((entry) => {
        const groupName = entry[0];
        const rows = entry[1];
        return new ClientSideGroupItem({
          childGroups: [] as IGroupTreeNode[],
          childRows: rows,
          columnId: groupingColumn,
          groupLabel: property!.name,
          rowCount: rows.length,
          parent: undefined,
          columnValue: groupName,
          columnDisplayValue: groupName,
          aggregations: this.calcAggregations(rows),
          grouper: this
        });
      });
  }

  private makeGroupMap(groupingColumn: string | undefined, rows: any[][]) {
    if(!groupingColumn){
      return new Map<string, any[][]>();
    }
    const index = this.findDataIndex(groupingColumn)
    const groupMap = new Map<string, any[][]>();
    for (let row of rows) {
      const groupName = row[index];
      if (!groupMap.has(groupName)) {
        groupMap.set(groupName, []);
      }
      groupMap.get(groupName)!.push(row)
    }
    return groupMap;
  }

  calcAggregations(rows: any[][]) {
    return getTablePanelView(this)
      .aggregations.aggregationList
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

  start(): void {
  }
}

