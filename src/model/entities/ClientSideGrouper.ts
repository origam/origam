import { IGrouper } from "./types/IGrouper";
import { getDataTable } from "model/selectors/DataView/getDataTable";
import { getGroupingConfiguration } from "model/selectors/TablePanelView/getGroupingConfiguration";
import { ICellOffset, IGroupTreeNode } from "gui/Components/ScreenElements/Table/TableRendering/types";
import { ClientSideGroupItem } from "gui/Components/ScreenElements/Table/TableRendering/GroupItem";
import { getTablePanelView } from "../selectors/TablePanelView/getTablePanelView";
import { IAggregationInfo } from "./types/IAggregationInfo";
import { computed } from "mobx";
import { AggregationType } from "./types/AggregationType";
import { getLocaleFromCookie } from "utils/cookies";
import { IProperty } from "./types/IProperty";
import { getAllLoadedValuesOfProp, getCellOffset, getNextRowId, getPreviousRowId, getRowById, getRowCount, getRowIndex } from "./GrouperCommon";
import { GroupingUnit, IGroupingSettings } from "./types/IGroupingConfiguration";
import moment from "moment";

export class ClientSideGrouper implements IGrouper {
  parent?: any = null;
  expandedGroupDisplayValues: Set<string> = new Set();

  @computed
  get topLevelGroups(){
    const firstGroupingColumn = getGroupingConfiguration(this).firstGroupingColumn;
    if (firstGroupingColumn === undefined) {
      return [];
    }
    const dataTable = getDataTable(this);
    const groups = this.makeGroups(undefined, dataTable.rows, firstGroupingColumn);
    this.loadRecursively(groups);
    console.log("topLevelGroups: "+groups.length);
    return groups;
  }
  
  get allGroups(){
    return this.topLevelGroups.flatMap(group => [group, ...group.allChildGroups]);
  }

  substituteRecord(row: any[]): void{}

  getCellOffset(rowId: string): ICellOffset {
   return getCellOffset(this, rowId);
  }
  
  getRowIndex(rowId: string): number | undefined {
    return getRowIndex(this, rowId);
  }

  getRowById(id: string): any[] | undefined {
    return getRowById(this, id);
  }

  getTotalRowCount(rowId: string): number | undefined {
    return getRowCount(this, rowId);
  }
  
  getAllValuesOfProp(property: IProperty): Promise<Set<any>> {
    return Promise.resolve(getAllLoadedValuesOfProp(property, this));
  }

  getNextRowId(rowId: string): string {
    return getNextRowId(this, rowId);
  }

  getPreviousRowId(rowId: string): string {
    return getPreviousRowId(this, rowId);
  }

  loadRecursively(groups: IGroupTreeNode[]) {
    for (let group of groups) {
      if(this.expandedGroupDisplayValues.has(group.columnDisplayValue)){
        group.isExpanded = true;
        this.loadChildrenInternal(group);
        this.loadRecursively(group.childGroups);
      }
    }
  }

  expansionListener(item: ClientSideGroupItem){
    if(item.isExpanded){
      this.expandedGroupDisplayValues.add(item.columnDisplayValue);
    }
    else
    {
      this.expandedGroupDisplayValues.delete(item.columnDisplayValue);
    }
  }

  makeGroups(parent: IGroupTreeNode | undefined, rows: any[][], groupingColumnSettings: IGroupingSettings): IGroupTreeNode[] {
    const dataTable = getDataTable(this);
    const property = dataTable.getPropertyById(groupingColumnSettings.columnId);

    return this.groupToGroupDataList(groupingColumnSettings, property!, rows)
      .sort((a, b) => a.compare(b))
      .map((groupData) => {
        return new ClientSideGroupItem({
          childGroups: [] as IGroupTreeNode[],
          childRows: groupData.rows,
          columnId: groupingColumnSettings.columnId,
          groupLabel: property!.name,
          rowCount: groupData.rows.length,
          parent: parent,
          columnValue: groupData.label,
          columnDisplayValue: property ? dataTable.resolveCellText(property, groupData.label) : groupData.label,
          aggregations: this.calcAggregations(groupData.rows),
          grouper: this,
          expansionListener: this.expansionListener.bind(this)
        });
      });
  }

  private groupToGroupDataList(groupingSettings: IGroupingSettings | undefined, property: IProperty, rows: any[][]) {
    if (!groupingSettings) {
      return [];
    }

    const index = this.findDataIndex(groupingSettings.columnId);
    const groupMap = new Map<string, GenericGroupData>();
    for (let row of rows) {    
      const groupData = groupingSettings.groupingUnit === undefined 
        ? new GenericGroupData(row[index])
        : DateGroupData.create( row[index], groupingSettings, property.formatterPattern)

      if (!groupMap.has(groupData.label)) {
        groupMap.set(groupData.label, groupData);
      }
      groupMap.get(groupData.label)!.rows!.push(row);
    }

    return Array.from(groupMap.values());
  }

  calcAggregations(rows: any[][]) {
    return getTablePanelView(this).aggregations.aggregationList.map((aggregationInfo) => {
      return {
        columnId: aggregationInfo.ColumnName,
        type: aggregationInfo.AggregationType,
        value: this.calcAggregation(aggregationInfo, rows),
      };
    });
  }

  private calcAggregation(aggregationInfo: IAggregationInfo, rows: any[][]) {
    const index = this.findDataIndex(aggregationInfo.ColumnName);
    const valuesToAggregate = rows.map((row) => row[index]);

    switch (aggregationInfo.AggregationType) {
      case AggregationType.SUM:
        return valuesToAggregate.reduce((a, b) => a + b, 0);
      case AggregationType.AVG:
        return valuesToAggregate.reduce((a, b) => a + b, 0) / rows.length;
      case AggregationType.MIN:
        return Math.min(...valuesToAggregate);
      case AggregationType.MAX:
        return Math.max(...valuesToAggregate);
      default:
        throw new Error("Aggregation type not implemented: " + aggregationInfo.AggregationType);
    }
  }

  findDataIndex(columnName: string) {
    const dataTable = getDataTable(this);
    const property = dataTable.getPropertyById(columnName);
    if (!property) {
      return 0;
    }
    return property.dataIndex;
  }

  *loadChildren(group: IGroupTreeNode) {
   this.loadChildrenInternal(group);
  }

  loadChildrenInternal(group: IGroupTreeNode){
    const groupingConfiguration = getGroupingConfiguration(this);
    const nextColumnName = groupingConfiguration.nextColumnToGroupBy(group.columnId);

    if (nextColumnName) {
      group.childGroups = this.makeGroups(group, group.childRows, nextColumnName);
    }
  }


  notifyGroupClosed(group: IGroupTreeNode){
  }

  start(): void {}
}


interface IGroupData{
  value: any;
  label: string;
  rows: any[][];
  compare(other: IGroupData): number;
}

class GenericGroupData implements IGroupData{
  constructor(
    public value: string
    ){
      this.label = value;
    }
    
  public label: any;
  public rows: any[][] = [];

  compare(other: IGroupData): number{
    if (this.label && other.label) {
      return this.label.localeCompare(other.label, getLocaleFromCookie());
    } else if (!this.label) {
      return -1;
    } else {
      return 1;
    }
  }
}

class DateGroupData implements IGroupData{
  constructor(
    public value: any,
    public label: string,
    ){
    }

  public rows: any[][] = [];

  public static create(value: string, groupingSettings: IGroupingSettings, formatterPattern: string): GenericGroupData{
    const momentValue = moment(value);
    if(!momentValue.isValid()){
      new DateGroupData(moment({ y:1900, M:1, d:1, h:0, m:0, s:0, ms:0 }), "");
    } 
    
    momentValue.format(formatterPattern);

    let format = formatterPattern;
    switch(groupingSettings.groupingUnit){
      case GroupingUnit.Year:
        momentValue.set({'month': 1, 'date': 1, 'hour': 0, 'minute': 0, 'second': 0});
        format = format
          .replace("MM","")
          .replace("DD","")
          .replace("h","")
          .replace("m","")
          .replace("s","")
          .replace(".","")
          .replace(":","")
        break;
      case GroupingUnit.Month:
        momentValue.set({'date': 1, 'hour': 0, 'minute': 0, 'second': 0});
        format = format
          .replace("DD.","")
          .replace("h","")
          .replace("m","")
          .replace("s","")
        break;
      case GroupingUnit.Day:
        momentValue.set({'hour': 0, 'minute': 0, 'second': 0});
        format = format
          .replace("h:","")
          .replace("m","")
          .replace("s","")
        break;
      case GroupingUnit.Hour:
        momentValue.set({'minute': 0, 'second': 0});
        format = format
          .replace("s","")
          .replace(":","")
        break;
      case GroupingUnit.Minute:
        momentValue.set({'second': 0});
        break;
    }
    const groupLabel = momentValue.format(format);
    return new DateGroupData(momentValue, groupLabel);
  }

  compare(other: IGroupData): number{
    if (this.value.isValid() && other.value.isValid()) {
      if (this.value > other.value){
        return 1;
      } 
      else if (this.value < other.value){
        return -1;
      }
      else{
        return 0;
      }
    } else if (!this.value) {
      return -1;
    } else {
      return 1;
    }
  }
}