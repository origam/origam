export interface IDropDownColumnData {
  id: string;
  name: string;
  entity: string;
  column: string;
  index: number;
}

export interface IDropDownColumn extends IDropDownColumnData {
  $type_IDropDownColumn: 1;

  parent?: any;
}

export const isDropDownColumn = (o: any): o is IDropDownColumn =>
  o.$type_IDropDownColumn;
