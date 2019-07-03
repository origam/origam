import { IDataSource, IDataSourceData } from "./types/IDataSource";
import { IDataSourceField } from "./types/IDataSourceField";

export class DataSource implements IDataSource {
  constructor(data: IDataSourceData) {
    Object.assign(this, data);
    this.fields.forEach(o => (o.parent = this));
  }

  parent?: any;

  entity: string = "";
  identifier: string = "";
  lookupCacheKey: string = "";
  fields: IDataSourceField[] = [];
}
