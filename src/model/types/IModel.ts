import { IDataTable } from "../entities/data/types/IDataTable";
import { ITableView } from '../entities/specificView/table/types/ITableView';
import { ICursor } from "../entities/cursor/types/ICursor";



export interface IDataTableQuery {
  dataViewId?: string;
}

export interface IDataViewQuery {
  dataViewId?: string;
}

export interface ICursorQuery {
  dataViewId?: string;
}

export interface IModel {
  getDataView(query: IDataViewQuery): ITableView | undefined;
  getDataTable(query: IDataTableQuery): IDataTable | undefined;
  getCursor(query: ICursorQuery): ICursor | undefined;
}