export interface IDataSourceFieldData {
  name: string;
  index: number;
}

export interface IDataSourceField extends IDataSourceFieldData {
  $type_IDataSourceField: 1;

  parent?: any;
}

export const isIDataSourceField = (o: any): o is IDataSourceField =>
  o.$type_IDataSourceField;
