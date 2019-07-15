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

  parent?: any;
  getFieldByName(name: string): IDataSourceField | undefined;
}

export const isIDataSource = (o: any): o is IDataSource => o.$type_IDataSource;
