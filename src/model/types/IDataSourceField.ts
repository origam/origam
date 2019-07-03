export interface IDataSourceFieldData {
  name: string;
  index: number;
}

export interface IDataSourceField extends IDataSourceFieldData {
  parent?: any;
}
