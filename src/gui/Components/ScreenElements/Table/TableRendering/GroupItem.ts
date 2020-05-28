
import {computed, observable} from "mobx";
import {IGroupTreeNode} from "./types";
import {IGrouper} from "../../../../../model/entities/types/IGrouper";
import {IAggregation} from "../../../../../model/entities/types/IAggregation";
import {getOrderingConfiguration} from "../../../../../model/selectors/DataView/getOrderingConfiguration";

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
    const orderingConfiguration = getOrderingConfiguration(this.grouper);

    if(orderingConfiguration.ordering.length === 0){
      return this._childRows;
    }else{
      return this._childRows.slice().sort(orderingConfiguration.orderingFunction());
    }
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
