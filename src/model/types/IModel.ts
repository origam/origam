
import { IDataViews } from "../entities/specificViews/types/IDataViews";

export interface IDataViewQuery {
  dataViewId?: string;
}

export interface IModel {
  getDataViews(query: IDataViewQuery): IDataViews | undefined;
  // getDataTable(query: IDataTableQuery): IDataTable | undefined;
  // getCursor(query: ICursorQuery): ICursor | undefined;
}
