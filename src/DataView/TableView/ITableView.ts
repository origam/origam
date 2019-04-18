import { IDataView } from "../types/IDataView";
import { IViewType } from "../types/IViewType";
import { IPropReorder } from '../types/IPropReorder';
import { IRecCursor } from "../types/IRecCursor";
import { IPropCursor } from "../types/IPropCursor";

export function isTableView(obj: any): obj is ITableView {
  return obj.type === IViewType.Table;
}

export interface ITableView {
  type: IViewType.Table;  
  dataView: IDataView;
  propReorder: IPropReorder;
  recCursor: IRecCursor;
  propCursor: IPropCursor;
}