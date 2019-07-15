export interface IAdditionalRowDataData {}

export interface IAdditionalRowData extends IAdditionalRowDataData {
  $type_IAdditionalRowData: 1;

  dirtyNew: boolean;
  dirtyDeleted: boolean;
  dirtyValues: Map<string, any>;
  dirtyFormValues: Map<string, any>;

  parent?: any;
}

export const isIAdditionalRowData = (o: any): o is IAdditionalRowData =>
  o.$type_IAdditionalRowData;
