
import {computed, observable} from "mobx";
import {IAggregation, IGroupTreeNode} from "./types";
import {IHeaderContainer} from "../../../../Workbench/ScreenArea/TableView/TableView";
import {parseAggregationType} from "../../../../../model/entities/types/IAggregationInfo";
import {IGrouper} from "../../../../../model/entities/types/IGrouper";
import {getDataTable} from "../../../../../model/selectors/DataView/getDataTable";

export interface IGroupItemData{
  childGroups: IGroupTreeNode[];
  childRows: any[][];
  columnId: string;
  columnValue: string ;
  columnDisplayValue: string ;
  groupLabel: string;
  parent: IGroupTreeNode | undefined;
  rowCount: number;
  aggregations: IAggregation[] | undefined;
  grouper: IGrouper;
}

export class GroupItem implements IGroupTreeNode {
  constructor(data: IGroupItemData) {
    Object.assign(this, data);
  }
  @observable childGroups: IGroupTreeNode[] = null as any;
  @observable _childRows: any[][] = null as any;
  columnId: string = null as any;
  columnValue: string = null as any;
  groupLabel: string = null as any;
  parent: IGroupTreeNode | undefined = null as any;
  rowCount: number = null as any;
  columnDisplayValue: string = null as any;
  aggregations: IAggregation[] | undefined = undefined;
  grouper: IGrouper = null as any;

  @observable isExpanded = false;

  @computed get childRows(){
    if(this.grouper.sortingFunction){
      const dataTable = getDataTable(this.grouper);
      return this._childRows.slice().sort(this.grouper.sortingFunction(dataTable));
    }
    return this._childRows;
  }
  set childRows(rows: any[][]){
    this._childRows = rows;
  }
}

export function parseAggregations(objectArray: any[] | undefined){
  if(!objectArray) return undefined;
  return objectArray.map(object =>
    {
      return {
        columnId: object["column"],
        type: parseAggregationType(object["type"]),
        value: object["value"]
      }
    });
}