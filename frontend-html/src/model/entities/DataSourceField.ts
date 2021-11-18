import {IDataSourceField, IDataSourceFieldData} from "./types/IDataSourceField";

export class DataSourceField implements IDataSourceField {
  $type_IDataSourceField: 1 = 1;
  
  constructor(data: IDataSourceFieldData) {
    Object.assign(this, data);
  }

  parent?: any;
  name: string = "";
  index: number = 0;
}
