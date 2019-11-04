export interface ITableDataManagerData {}

export interface ITableDataManager extends ITableDataManagerData {
  $type_ITableDataManager: 1;

  parent?: any;
}

export const isITableDataManager = (o: any): o is ITableDataManager =>
  o.$type_ITableDataManager;
