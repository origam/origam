
import { observable } from "mobx";
import {IAggregation, IGroupTreeNode} from "./types";
import {IHeaderContainer} from "../../../../Workbench/ScreenArea/TableView/TableView";
import {aggregationTypeParse} from "../../../../../model/entities/types/IAggregationInfo";

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

  @observable isExpanded = false;
}

export function parseAggregations(objectArray: any[] | undefined){
  if(!objectArray) return undefined;
  return objectArray.map(object =>
    {
      return {
        columnId: object["column"],
        type: aggregationTypeParse(object["type"]),
        value: object["value"]
      }
    });
}