import { IToolbar } from "./IToolbar";
import { ITable } from "./ITable";
import { IViewType } from "../IScreenPresenter";


export interface ITableView {
  type: IViewType.TableView;
  toolbar: IToolbar | undefined;
  table: ITable;
}
