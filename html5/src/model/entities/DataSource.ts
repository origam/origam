import { IDataSource, IDataSourceData } from "./types/IDataSource";
import { IDataSourceField } from "./types/IDataSourceField";

export class DataSource implements IDataSource {
  $type_IDataSource: 1 = 1;
  
  constructor(data: IDataSourceData) {
    Object.assign(this, data);
    this.fields.forEach(o => (o.parent = this));
  }

  parent?: any;

  entity: string = "";
  dataStructureEntityId: string = "";
  identifier: string = "";
  lookupCacheKey: string = "";
  fields: IDataSourceField[] = [];

  getFieldByName(name: string): IDataSourceField | undefined {
    return this.fields.find(field => field.name === name);
  }
}
