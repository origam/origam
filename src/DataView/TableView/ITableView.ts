import { IViewType } from "../types/IViewType";
import { IPropReorder } from "../types/IPropReorder";
import { IRecCursor } from "../types/IRecCursor";
import { IPropCursor } from "../types/IPropCursor";
import { IAActivateView } from "../types/IAActivateView";
import { IADeactivateView } from "../types/IADeactivateView";
import { IDataViewMediator02 } from "../DataViewMediator02";
import { ISelection } from "../Selection";

export function isTableView(obj: any): obj is ITableView {
  return obj.type === IViewType.Table;
}

export interface ITableView {
  type: IViewType.Table;
  dataView: IDataViewMediator02;
  propReorder: IPropReorder;
  recCursor: IRecCursor;
  propCursor: IPropCursor;
  aActivateView: IAActivateView;
  aDeactivateView: IADeactivateView;
  selection: ISelection;
}
