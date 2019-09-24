import { IDataSourceField } from "./IDataSourceField";

export interface IDataSourceData {
  entity: string;
  identifier: string;
  lookupCacheKey: string;
  fields: IDataSourceField[];
  dataStructureEntityId: string;
}

export interface IDataSource extends IDataSourceData {
  $type_IDataSource: 1;

  getFieldByName(name: string): IDataSourceField | undefined;
  getFieldByIndex(idex: number): IDataSourceField | undefined;

  parent?: any;
}

export const isIDataSource = (o: any): o is IDataSource => o.$type_IDataSource;
