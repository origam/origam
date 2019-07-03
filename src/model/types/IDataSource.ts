import { IDataSourceField } from "./IDataSourceField";

export interface IDataSourceData {
  entity: string;
  identifier: string;
  lookupCacheKey: string;
  fields: IDataSourceField[];
}

export interface IDataSource extends IDataSourceData {
  parent?: any;
}
