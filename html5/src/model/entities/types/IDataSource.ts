import { IDataSourceField } from "./IDataSourceField";
import { IRowState } from "./IRowState";

export interface IDataSourceData {
  entity: string;
  identifier: string;
  lookupCacheKey: string;
  fields: IDataSourceField[];
  dataStructureEntityId: string;
  rowState: IRowState;
}

export interface IDataSource extends IDataSourceData {
  $type_IDataSource: 1;

  getFieldByName(name: string): IDataSourceField | undefined;
  getFieldByIndex(idex: number): IDataSourceField | undefined;

  parent?: any;
}

export const isIDataSource = (o: any): o is IDataSource => o.$type_IDataSource;
