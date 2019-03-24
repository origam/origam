import { IModel, IDataViewQuery, IDataTableQuery, ICursorQuery } from "../types/IModel";
import { ITableView } from "./specificView/table/types/ITableView";
import { IDataTable } from "./data/types/IDataTable";
import { ICursor } from "./cursor/types/ICursor";



export class Model implements IModel {
  constructor(tableViews: ITableView[]) {
    this.tableViews = tableViews;
  }

  tableViews: ITableView[];

  getDataView(query: IDataViewQuery): ITableView | undefined {
    if (query.dataViewId) {
      return this.tableViews.find(obj => obj.id === query.dataViewId);
    }
    return;
  }

  getDataTable(query: IDataTableQuery): IDataTable | undefined {
    const dataView = this.getDataView(query);
    return dataView && dataView.dataTable;
  }

  getCursor(query: ICursorQuery): ICursor | undefined {
    const dataView = this.getDataView(query);
    return dataView && dataView.cursor;
  }
}
