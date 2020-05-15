
import {computed, observable} from "mobx";
import {IGroupTreeNode} from "./types";
import {IGrouper} from "../../../../../model/entities/types/IGrouper";
import {getDataTable} from "../../../../../model/selectors/DataView/getDataTable";
import {IAggregation} from "../../../../../model/entities/types/IAggregation";

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

export class ClientSideGroupItem implements IGroupTreeNode {
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
    const dataTable = getDataTable(this.grouper);
    if(dataTable.sortingFn){
      return this._childRows.slice().sort(dataTable.sortingFn(dataTable));
    }
    return this._childRows;
  }
  set childRows(rows: any[][]){
    this._childRows = rows;
  }
}

export class GroupItem implements IGroupTreeNode {
  constructor(data: IGroupItemData) {
    Object.assign(this, data);
  }
  @observable childGroups: IGroupTreeNode[] = null as any;
  @observable childRows: any[][] = null as any;
  columnId: string = null as any;
  columnValue: string = null as any;
  groupLabel: string = null as any;
  parent: IGroupTreeNode | undefined = null as any;
  rowCount: number = null as any;
  columnDisplayValue: string = null as any;
  aggregations: IAggregation[] | undefined = undefined;
  grouper: IGrouper = null as any;

  @observable isExpanded = false;
}
